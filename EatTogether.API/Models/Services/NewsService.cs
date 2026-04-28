using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Repositories;

namespace EatTogether.API.Models.Services
{
	public interface INewsService
	{
		Task<NewsPagedResultDto<NewsListDto>> GetNewsListAsync(int page, int pageSize, string? categoryName);
		Task<NewsDetailResultDto?> GetNewsDetailAsync(int id);
		Task<bool> IncrementViewCountAsync(int id);
	}

	public class NewsService : INewsService
	{
		private readonly INewsRepository _newsRepo;

		public NewsService(INewsRepository newsRepo)
		{
			_newsRepo = newsRepo;
		}

		public async Task<NewsPagedResultDto<NewsListDto>> GetNewsListAsync(int page, int pageSize, string? categoryName)
		{
			int totalCount = await _newsRepo.GetNewsCountAsync(categoryName);
			List<Article> articles = await _newsRepo.GetNewsListAsync(page, pageSize, categoryName);

			List<NewsListDto> newsList = articles.Select(n => new NewsListDto
			{
				Id = n.Id,
				CategoryName = n.Category.Name,
				Title = n.Title,
				Summary = n.Description != null
						  ? (n.Description.Length > 100 ? n.Description.Substring(0, 100) + "…" : n.Description)
						  : "",
				CoverImageUrl = !string.IsNullOrWhiteSpace(n.CoverImageUrl)
								? "/images/articles/" + n.CoverImageUrl
								: "/images/articles/article-14.jpg",  //預設公告圖片
				PublishDate = n.PublishDate,
				IsPinned = n.IsPinned,
				ViewCount = n.ViewCount
			}).ToList();

			return new NewsPagedResultDto<NewsListDto>
			{
				Data = newsList,
				Page = page,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
			};
		}

		public async Task<NewsDetailResultDto?> GetNewsDetailAsync(int id)
		{
			Article? article = await _newsRepo.GetNewsDetailAsync(id);
			if (article == null) return null;

			NewsDetailDto detail = new NewsDetailDto
			{
				Id = article.Id,
				CategoryName = article.Category.Name,
				Title = article.Title,
				Description = article.Description,
				CoverImageUrl = !string.IsNullOrWhiteSpace(article.CoverImageUrl)
								? "/images/articles/" + article.CoverImageUrl
								: "/images/articles/article-14.jpg", //預設公告圖片
				PublishDate = article.PublishDate,
				ViewCount = article.ViewCount,
				IsPinned = article.IsPinned
			};

			if (article.PublishDate == null) return null;

			var prev = await _newsRepo.GetPrevArticleAsync(article.PublishDate.Value);
			var next = await _newsRepo.GetNextArticleAsync(article.PublishDate.Value);

			return new NewsDetailResultDto
			{
				Article = detail,
				Prev = prev == null ? null : new NewsNavDto { Id = prev.Id, Title = prev.Title },
				Next = next == null ? null : new NewsNavDto { Id = next.Id, Title = next.Title }
			};
		}

		public async Task<bool> IncrementViewCountAsync(int id)
		{
			int affected = await _newsRepo.IncrementViewCountAsync(id);
			return affected > 0;
		}
	}
}
