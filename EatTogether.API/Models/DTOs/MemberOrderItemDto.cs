namespace EatTogether.Models.DTOs
{
    public class MemberOrderItemDto
    {
        public string ProductName { get; set; } = "";
        public int Qty { get; set; }
        public string? Note { get; set; }
    }
}
