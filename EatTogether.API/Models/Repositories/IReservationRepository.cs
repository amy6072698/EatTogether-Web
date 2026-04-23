using EatTogether.Models.DTOs;

namespace EatTogether.Models.Repositories
{
    public interface IReservationRepository
    {
        /// <summary>取得會員所有訂位（依訂位日期倒序）</summary>
        Task<List<ReservationListDto>> GetByMemberIdAsync(int memberId);

        /// <summary>訪客憑訂位單號 + Email 查詢單筆訂位</summary>
        Task<ReservationDetailDto?> GetByBookingNumberAndEmailAsync(string bookingNumber, string email);

        /// <summary>依 Id 查詢訂位詳情</summary>
        Task<ReservationDetailDto?> GetByIdAsync(int id);

        /// <summary>建立訂位，回傳新建的 Id</summary>
        Task<int> CreateAsync(ReservationCreateDto dto, string bookingNumber, int? memberId);

        /// <summary>取消訂位（設定 Status=2, CancelledAt=now）</summary>
        Task CancelAsync(int id);

        /// <summary>取得指定日期當日已使用的最大流水號（用於產生 BookingNumber）</summary>
        Task<int> GetMaxSeqOfDayAsync(DateTime date);

        /// <summary>取得時段（±90 分鐘）內所有訂位的人數組合（Status IN (0,1)）</summary>
        Task<List<(int Adults, int Children)>> GetSessionReservationsAsync(DateTime start, DateTime end);

        /// <summary>檢查指定會員在 ±90 分鐘內是否已有訂位</summary>
        Task<bool> IsConflictForMemberAsync(DateTime reservationDate, int memberId);

        /// <summary>取得會員累積 No-Show 次數</summary>
        Task<int> GetNoShowCountAsync(int memberId);

        /// <summary>將指定訂位標記為 No-Show（Status=3）</summary>
        Task MarkNoShowAsync(int reservationId);

        /// <summary>取得需要標記為 No-Show 的訂位 Id 清單（Status=0, ReservationDate < now-10min）</summary>
        Task<List<int>> GetPendingNoShowIdsAsync();

        /// <summary>取得需要寄 24hr 提醒的訂位（ReservationDate 在 23h~25h 後，Status IN (0,1)）</summary>
        Task<List<ReservationDetailDto>> GetRemindersAsync(DateTime from, DateTime to);
    }
}
