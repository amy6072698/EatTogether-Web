using EatTogether.API.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.API.Models.Repositories
{
	public class NewsRepository : INewsRepository
	{
		private readonly EatTogetherDBContext _context;

		public NewsRepository(EatTogetherDBContext context)
		{
			_context = context;
		}

		public async Task<int> GetNewsCountAsync(string? categoryName)
		{
			var now = DateTime.Now;
			IQueryable<Article> query = _context.Articles
				.Where(n => n.Status == 1 && n.PublishDate <= now);

			if (!string.IsNullOrEmpty(categoryName))
				query = query.Where(n => n.Category.Name == categoryName);

			return await query.CountAsync();
		}

		public async Task<List<Article>> GetNewsListAsync(int page, int pageSize, string? categoryName)
		{
			var now = DateTime.Now;
			IQueryable<Article> query = _context.Articles
				.Include(n => n.Category)
				.Where(n => n.Status == 1 && n.PublishDate <= now);

			if (!string.IsNullOrEmpty(categoryName))
				query = query.Where(n => n.Category.Name == categoryName);

			return await query
				.OrderByDescending(n => n.IsPinned)
				.ThenByDescending(n => n.PublishDate)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

		public async Task<Article?> GetNewsDetailAsync(int id)
		{
			var now = DateTime.Now;
			return await _context.Articles
				.Include(n => n.Category)
				.Where(n => n.Id == id && n.Status == 1 && n.PublishDate <= now)
				.FirstOrDefaultAsync();
		}

		public async Task<Article?> GetPrevArticleAsync(DateTime publishDate)
		{
			var now = DateTime.Now;
			return await _context.Articles
				.Where(n => n.Status == 1 && n.PublishDate <= now && n.PublishDate > publishDate)
				.OrderBy(n => n.PublishDate)
				.FirstOrDefaultAsync();
		}

		public async Task<Article?> GetNextArticleAsync(DateTime publishDate)
		{
			var now = DateTime.Now;
			return await _context.Articles
				.Where(n => n.Status == 1 && n.PublishDate <= now && n.PublishDate < publishDate)
				.OrderByDescending(n => n.PublishDate)
				.FirstOrDefaultAsync();
		}

		public async Task<int> IncrementViewCountAsync(int id)
		{
			return await _context.Articles
				.Where(a => a.Id == id)
				.ExecuteUpdateAsync(s => s.SetProperty(a => a.ViewCount, a => a.ViewCount + 1));
		}
	}
}
