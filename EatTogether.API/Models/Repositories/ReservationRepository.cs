using EatTogether.API.Models.EfModels;
using EatTogether.Models.DTOs;
using EatTogether.Models.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.Models.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly EatTogetherDBContext _context;

        public ReservationRepository(EatTogetherDBContext context)
        {
            _context = context;
        }

        public async Task<List<ReservationListDto>> GetByMemberIdAsync(int memberId)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r => r.MemberId == memberId)
                .OrderByDescending(r => r.ReservationDate)
                .Select(r => r.ToListDto())
                .ToListAsync();
        }

        public async Task<ReservationDetailDto?> GetByBookingNumberAndEmailAsync(string bookingNumber, string email)
        {
            var r = await _context.Reservations
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.BookingNumber == bookingNumber &&
                    r.Email == email);

            return r?.ToDetailDto();
        }

        public async Task<ReservationDetailDto?> GetByIdAsync(int id)
        {
            var r = await _context.Reservations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            return r?.ToDetailDto();
        }

        public async Task<int> CreateAsync(ReservationCreateDto dto, string bookingNumber, int? memberId)
        {
            var entity = new Reservation
            {
                BookingNumber   = bookingNumber,
                Name            = dto.Name,
                Phone           = dto.Phone,
                Email           = dto.Email,
                ReservationDate = dto.ReservationDate,
                AdultsCount     = dto.AdultsCount,
                ChildrenCount   = dto.ChildrenCount,
                Status          = 0,
                Remark          = dto.Remark,
                ReservedAt      = DateTime.Now,
                MemberId        = memberId
            };

            _context.Reservations.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task CancelAsync(int id)
        {
            var entity = await _context.Reservations.FindAsync(id);
            if (entity == null) return;

            entity.Status      = 2;
            entity.CancelledAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetMaxSeqOfDayAsync(DateTime date)
        {
            // BookingNumber 格式：R + yyMMdd + seq(3碼)，e.g. R260311006
            // 前綴為 R + 年後2碼 + 月2碼 + 日2碼 = 7 字元
            var prefix = $"R{date:yyMMdd}";

            var numbers = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.BookingNumber.StartsWith(prefix))
                .Select(r => r.BookingNumber)
                .ToListAsync();

            if (!numbers.Any()) return 0;

            return numbers
                .Select(bn => int.TryParse(bn[7..], out var seq) ? seq : 0)
                .Max();
        }

        public async Task<List<(int Adults, int Children)>> GetSessionReservationsAsync(DateTime start, DateTime end)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r =>
                    r.ReservationDate >= start &&
                    r.ReservationDate <= end &&
                    (r.Status == 0 || r.Status == 1))
                .Select(r => ValueTuple.Create(r.AdultsCount, r.ChildrenCount))
                .ToListAsync();
        }

        public async Task<bool> IsConflictForMemberAsync(DateTime reservationDate, int memberId)
        {
            var windowStart = reservationDate.AddMinutes(-90);
            var windowEnd   = reservationDate.AddMinutes(90);

            return await _context.Reservations
                .AnyAsync(r =>
                    r.MemberId == memberId &&
                    r.ReservationDate >= windowStart &&
                    r.ReservationDate <= windowEnd &&
                    (r.Status == 0 || r.Status == 1));
        }

        public async Task<int> GetNoShowCountAsync(int memberId)
        {
            return await _context.Reservations
                .CountAsync(r => r.MemberId == memberId && r.Status == 3);
        }

        public async Task MarkNoShowAsync(int reservationId)
        {
            var entity = await _context.Reservations.FindAsync(reservationId);
            if (entity == null) return;

            entity.Status = 3;
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetPendingNoShowIdsAsync()
        {
            var cutoff = DateTime.Now.AddMinutes(-10);
            return await _context.Reservations
                .AsNoTracking()
                .Where(r => r.Status == 0 && r.ReservationDate < cutoff)
                .Select(r => r.Id)
                .ToListAsync();
        }

        public async Task<List<ReservationDetailDto>> GetRemindersAsync(DateTime from, DateTime to)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r =>
                    r.ReservationDate >= from &&
                    r.ReservationDate <= to &&
                    (r.Status == 0 || r.Status == 1) &&
                    r.Email != null)
                .Select(r => r.ToDetailDto())
                .ToListAsync();
        }
    }
}
