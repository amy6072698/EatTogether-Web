using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace EatTogether.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NewsController : ControllerBase
	{
		private readonly INewsService _service;
		private readonly IMemoryCache _cache;

		public NewsController(INewsService service, IMemoryCache cache)
		{
			_service = service;
			_cache = cache;
		}

		/// <summary>查看文章列表</summary>
		// GET: api/News
		[HttpGet]
		public async Task<ActionResult<NewsPagedResultDto<NewsListDto>>> GetNewsList(
			int page = 1, int pageSize = 5, string? categoryName = null)
		{
			var result = await _service.GetNewsListAsync(page, pageSize, categoryName);
			return Ok(result);
		}

		/// <summary>查看單篇文章</summary>
		// GET: api/News/5
		[HttpGet("{id}")]
		public async Task<ActionResult<NewsDetailResultDto>> GetNewsDetail(int id)
		{
			var result = await _service.GetNewsDetailAsync(id);
			if (result == null) return NotFound();
			return Ok(result);
		}

		/// <summary>增加文章瀏覽次數</summary>
		// POST: api/News/5/view
		[HttpPost("{id}/view")]
		public async Task<ActionResult> IncrementViewCount(int id)
		{
			// 預防短時間點擊去刷閱覽數，以cache去抓
			// 登入者用 memberId，未登入用 VisitorId，都沒有才 fallback IP
			var memberId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var visitorId = Request.Headers["X-Visitor-Id"].FirstOrDefault();

			var viewerKey = !string.IsNullOrWhiteSpace(memberId)
				? $"user_{memberId}"
				: !string.IsNullOrWhiteSpace(visitorId)
					? $"visitor_{visitorId}"
					: $"ip_{HttpContext.Connection.RemoteIpAddress}";

			var cacheKey = $"view_{viewerKey}_{id}";

			// 後端再擋一次（防止繞過前端直接打 API）
			if (_cache.TryGetValue(cacheKey, out _))
				return NoContent();

			_cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));

			var success = await _service.IncrementViewCountAsync(id);
			if (!success) return NotFound();

			return NoContent();
		}
	}
}
