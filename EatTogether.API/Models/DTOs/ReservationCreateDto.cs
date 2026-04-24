namespace EatTogether.Models.DTOs
{
    public class ReservationCreateDto
    {
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public DateTime ReservationDate { get; set; }
        public int AdultsCount { get; set; } = 2;
        public int ChildrenCount { get; set; } = 0;
        public string? Remark { get; set; }
    }
}
