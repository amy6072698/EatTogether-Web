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
		public async Task<ActionResult<NewsPagedResultDto<NewsListDto>>> GetNewsList(int page = 1,
	int pageSize = 10)  //預設每頁10筆，頁碼從1開始
		{
			var now = DateTime.Now;

			IQueryable<Article> query = _context.Articles
				.Where(n => n.Status == 1 && n.PublishDate <= now); // 只顯示已發佈、日期已到發佈日期的文章

			int totalCount = await query.CountAsync();   //計算總數量

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
					CoverImageUrl = n.CoverImageUrl,
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
					CoverImageUrl = n.CoverImageUrl,
					PublishDate = n.PublishDate,
					ViewCount = n.ViewCount
				})
				.FirstOrDefaultAsync();

			if (news == null)
			{
				return NotFound();
			}

			return Ok(news);
		}

	}
}
