namespace EatTogether.Models.DTOs
{
    public class TableTypeSlotDto
    {
        public string TableType { get; set; } = null!;
        public int SeatCount { get; set; }
        public int Total { get; set; }
        public int Available { get; set; }
    }

    public class AvailabilityDto
    {
        public bool IsAvailable { get; set; }
        public int RemainingCapacity { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<TableTypeSlotDto> TableTypeAvailability { get; set; } = new();
    }
}
