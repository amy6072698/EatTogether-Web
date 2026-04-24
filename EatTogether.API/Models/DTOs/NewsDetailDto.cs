namespace EatTogether.API.Models.DTOs
{
	public class NewsDetailDto
	{
		public int Id { get; set; }

		public string CategoryName { get; set; }  // 文章類別名稱

		public string Title { get; set; }

		public string Description { get; set; }

		public string CoverImageUrl { get; set; }

		public DateTime? PublishDate { get; set; }

		public int ViewCount { get; set; }

		public bool IsPinned { get; set; }



	}
}
