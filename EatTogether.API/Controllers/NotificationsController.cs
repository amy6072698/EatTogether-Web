using EatTogether.API.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace EatTogether.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class NotificationsController : ControllerBase
	{
		private readonly INotificationService _service;

		public NotificationsController(INotificationService service)
		{
			_service = service;
		}

		/// <summary>
		/// 取得登入者的通知列表
		/// </summary>
		// GET: api/Notifications
		[HttpGet]
		public async Task<IActionResult> GetNotifications()
		{
			var memberId = GetMemberId();
			if (memberId == null) return Unauthorized();

			var result = await _service.GetNotificationsAsync(memberId.Value);
			return Ok(result);
		}

		/// <summary>
		/// 標記單一通知為已讀
		/// </summary>
		// PATCH: api/Notifications/{id}/read
		[HttpPatch("{id}/read")]
		public async Task<IActionResult> MarkAsRead(int id)
		{
			var memberId = GetMemberId();
			if (memberId == null) return Unauthorized();

			await _service.MarkAsReadAsync(id, memberId.Value);
			return NoContent();

		}

		/// <summary>
		/// 標記所有通知為已讀
		/// </summary>
		// PATCH: api/Notifications/read-all
		[HttpPatch("read-all")]
		public async Task<IActionResult> MarkAllAsRead()
		{
			var memberId = GetMemberId();
			if (memberId == null) return Unauthorized();
			await _service.MarkAllAsReadAsync(memberId.Value);
			return NoContent();
		}

		/// <summary>
		/// 從 JWT 取得 MemberId
		/// </summary>
		private int? GetMemberId()
		{
			var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			return int.TryParse(claim, out var id) ? id : null;
		}

	}
}
