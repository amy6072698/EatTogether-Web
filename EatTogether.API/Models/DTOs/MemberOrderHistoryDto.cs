namespace EatTogether.Models.DTOs
{
    public class MemberOrderHistoryDto
    {
        public string OrderNumber { get; set; } = "";
        public DateTime OrderAt { get; set; }
        public string OrderNote { get; set; } = "";
        public List<MemberOrderItemDto> Items { get; set; } = new();
    }
}
