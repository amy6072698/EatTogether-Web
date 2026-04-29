namespace EatTogether.API.Models.DTOs
{
	public class NotificationDto
	{
		public int Id { get; set; }
		public int ArticleId { get; set; }
		public string Title { get; set; } = string.Empty;
		public bool IsRead { get; set; }
		public DateTime CreatedAt { get; set; }

	}
}
