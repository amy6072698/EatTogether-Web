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

		// 取得登入者的通知列表
		// GET: api/Notifications
		[HttpGet]
		public async Task<IActionResult> GetNotifications()
		{
			var memberId = GetMemberId();
			if (memberId == null) return Unauthorized();

			var result = await _service.GetNotificationsAsync(memberId.Value);
			return Ok(result);
		}

		// 標記單一通知為已讀
		// PATCH: api/Notifications/{id}/read
		[HttpPatch("{id}/read")]
		public async Task<IActionResult> MarkAsRead(int Id)
		{
			var memberId = GetMemberId();
			if (memberId == null) return Unauthorized();

			await _service.MarkAsReadAsync(Id, memberId.Value);
			return NoContent();

		}

		// 標記所有通知為已讀
		// PATCH: api/Notifications/read-all
		[HttpPatch("read-all")]
		public async Task<IActionResult> MarkAllAsRead()
		{
			var memberId = GetMemberId();
			if (memberId == null) return Unauthorized();
			await _service.MarkAllAsReadAsync(memberId.Value);
			return NoContent();
		}




		// 從 JWT 取得 MemberId
		private int? GetMemberId()
		{
			var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
			return int.TryParse(claim, out var id) ? id : null;
		}

	}
}
