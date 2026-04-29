namespace EatTogether.API.Models.DTOs
{
	public class NewsDetailResultDto
	{
		public NewsDetailDto Article { get; set; } = null!;
		public NewsNavDto? Prev { get; set; }
		public NewsNavDto? Next { get; set; }
	}
}
