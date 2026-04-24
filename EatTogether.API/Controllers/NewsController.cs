using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NewsController : ControllerBase
	{
		private readonly EatTogetherDBContext _context;

		public NewsController(EatTogetherDBContext context)
		{
			_context = context;
		}

		/// <summary>查看文章列表</summary>
		// GET: api/News
		[HttpGet]
		public async Task<ActionResult<NewsPagedResultDto<NewsListDto>>> GetNewsList(int page = 1, int pageSize = 5,	string? categoryName = null)  
		{
			var now = DateTime.Now;

			IQueryable<Article> query = _context.Articles
				.Where(n => n.Status == 1 && n.PublishDate <= now); // 只顯示已發佈、日期已到發佈日期的文章

			//有傳分類才過濾
			if (!string.IsNullOrEmpty(categoryName))
			{
				query = query.Where(n => n.Category.Name == categoryName);
			}

			int totalCount = await query.CountAsync();   //篩選後才算數量

			List<NewsListDto> newsList = await query
				.OrderByDescending(n => n.IsPinned)
				.ThenByDescending(n => n.PublishDate)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(n => new NewsListDto
				{
					Id = n.Id,
					CategoryName = n.Category.Name,
					Title = n.Title,
					Summary = n.Description != null
							  ? (n.Description.Length > 100 ? n.Description.Substring(0, 100) + "…" : n.Description)
							  : "",
					CoverImageUrl = n.CoverImageUrl != null   
								? "/images/articles/" + n.CoverImageUrl
								: "",
					PublishDate = n.PublishDate,
					IsPinned = n.IsPinned,
					ViewCount = n.ViewCount
				})
				.ToListAsync();

			NewsPagedResultDto<NewsListDto> result = new NewsPagedResultDto<NewsListDto>
			{
				Data = newsList,
				Page = page,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
			};
			return Ok(result);

		}

		/// <summary>查看單篇文章</summary>
		// GET: api/News/5
		[HttpGet("{id}")]
		public async Task<ActionResult<NewsDetailDto>> GetNewsDetail(int id)
		{

			var now = DateTime.Now;

			var news = await _context.Articles
				.Where(n=> n.Id == id
					&& n.Status == 1
					&& n.PublishDate <= now
				)
				.Select(n => new NewsDetailDto
				{
					Id = n.Id,
					CategoryName = n.Category.Name,
					Title = n.Title,
					Description = n.Description,
					CoverImageUrl = n.CoverImageUrl != null   
									? "/images/articles/" + n.CoverImageUrl
									: "",
					PublishDate = n.PublishDate,
					ViewCount = n.ViewCount
				})
				.FirstOrDefaultAsync();

			if (news == null) return NotFound();

			// 上一篇：發佈日期比當前文章晚的最舊一篇
			var prev = await _context.Articles
				.Where(n => n.Status == 1 && n.PublishDate <= now && n.PublishDate > news.PublishDate)
				.OrderBy(n => n.PublishDate)
				.Select(n => new { n.Id, n.Title })
				.FirstOrDefaultAsync();
				

			// 下一篇：發佈日期比當前文章早的最新一篇
			var next = await _context.Articles
				.Where(n => n.Status == 1 && n.PublishDate <= now && n.PublishDate < news.PublishDate)
				.OrderByDescending(n => n.PublishDate)
				.Select(n => new { n.Id, n.Title })
				.FirstOrDefaultAsync();

			return Ok(new
			{
				article = news,
				prev,
				next
			});
		}

	}
}
