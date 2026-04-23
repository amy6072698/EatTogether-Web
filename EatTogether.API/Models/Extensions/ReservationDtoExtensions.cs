using EatTogether.API.Models.EfModels;
using EatTogether.Models.DTOs;

namespace EatTogether.Models.Extensions
{
    public static class ReservationDtoExtensions
    {
        public static ReservationListDto ToListDto(this Reservation r) => new()
        {
            Id           = r.Id,
            BookingNumber = r.BookingNumber,
            Name         = r.Name,
            ReservationDate = r.ReservationDate,
            AdultsCount  = r.AdultsCount,
            ChildrenCount = r.ChildrenCount,
            Status       = r.Status,
            CancelledAt  = r.CancelledAt
        };

        public static ReservationDetailDto ToDetailDto(this Reservation r) => new()
        {
            Id            = r.Id,
            BookingNumber = r.BookingNumber,
            Name          = r.Name,
            Phone         = r.Phone,
            Email         = r.Email,
            ReservationDate = r.ReservationDate,
            AdultsCount   = r.AdultsCount,
            ChildrenCount = r.ChildrenCount,
            Status        = r.Status,
            Remark        = r.Remark,
            ReservedAt    = r.ReservedAt,
            CancelledAt   = r.CancelledAt,
            MemberId      = r.MemberId
        };
    }
}
