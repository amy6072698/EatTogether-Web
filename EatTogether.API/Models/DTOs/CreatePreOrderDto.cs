namespace EatTogether.Models.DTOs
{
    public class CreatePreOrderDto
    {
        public int TableId { get; set; }
        public bool InOrOut { get; set; }
        public int? PeopleNum { get; set; }
        public bool IsAddOrder { get; set; }
        public string PayMethod { get; set; }
        public string? Note { get; set; }
        // 外帶專用：顧客姓名、電話、取餐時段
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? PickupTime { get; set; }
        public int DiscountAmount { get; set; }
        public int? CouponId { get; set; }
        public int? EventId { get; set; }
        public int? MemberId { get; set; }
        public int? UserId { get; set; }
        public List<PreOrderDetailDto> Items { get; set; } = new();
    }
}
