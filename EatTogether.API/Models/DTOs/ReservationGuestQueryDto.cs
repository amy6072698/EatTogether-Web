namespace EatTogether.Models.DTOs
{
    public class ReservationGuestQueryDto
    {
        public string BookingNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class ReservationCancelDto
    {
        /// <summary>訪客取消時需帶入；會員取消時從 JWT 驗證，可不帶</summary>
        public string? BookingNumber { get; set; }
        public string? Email { get; set; }
    }
}
