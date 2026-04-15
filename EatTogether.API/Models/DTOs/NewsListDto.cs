namespace EatTogether.API.Models.DTOs
{

	public class NewsListDto
	{
		public int Id { get; set; }

		public string CategoryName { get; set; }  // 文章類別名稱

		public string Title { get; set; }

		public string Summary { get; set; }   // 存最新消息的文章摘要

		public string CoverImageUrl { get; set; }

		public DateTime? PublishDate { get; set; }

		public bool IsPinned { get; set; }

		public int ViewCount { get; set; }

	}
}
