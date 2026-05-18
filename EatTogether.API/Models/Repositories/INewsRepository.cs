using EatTogether.API.Models.EfModels;

namespace EatTogether.API.Models.Repositories
{
	public interface INewsRepository
	{
		// 對應 GetNewsList
		Task<int> GetNewsCountAsync(string? categoryName);
		Task<List<Article>> GetNewsListAsync(int page, int pageSize, string? categoryName);

		// 對應 GetNewsDetail
		Task<Article?> GetNewsDetailAsync(int id);
		Task<Article?> GetPrevArticleAsync(DateTime publishDate);
		Task<Article?> GetNextArticleAsync(DateTime publishDate);

		// 對應 IncrementViewCount
		Task<int> IncrementViewCountAsync(int id);
	}
}
