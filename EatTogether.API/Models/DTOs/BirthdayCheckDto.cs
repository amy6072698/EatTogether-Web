namespace EatTogether.Models.DTOs
{
    public class BirthdayCheckDto
    {
        public bool HasBirthdayCoupon { get; set; }
        public string? CouponName { get; set; }
        public string? Code { get; set; }
        public string? DiscountDescription { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysUntilExpiry { get; set; }
    }
}
