namespace EatTogether.Models.DTOs
{
    public class ReservationDetailDto
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public DateTime ReservationDate { get; set; }
        public int AdultsCount { get; set; }
        public int ChildrenCount { get; set; }
        public int Status { get; set; }
        public string? Remark { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public int? MemberId { get; set; }

        public string StatusText => Status switch
        {
            0 => "訂位中",
            1 => "已報到",
            2 => "已取消",
            3 => "No-Show",
            _ => "未知"
        };

        public string StatusBadgeClass => Status switch
        {
            0 => "bg-primary",
            1 => "bg-success",
            2 => "bg-danger",
            3 => "bg-secondary",
            _ => "bg-light text-dark"
        };

        /// <summary>是否可取消（Status=0 且距訂位時間 > 1 小時）</summary>
        public bool CanCancel =>
            Status == 0 && ReservationDate > DateTime.Now.AddHours(1);
    }
}
