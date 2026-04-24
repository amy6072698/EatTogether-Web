using EatTogether.API.Models.DTOs;
using EatTogether.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EatTogether.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("login")]
		[EnableRateLimiting("AuthPolicy")]
		public IActionResult Login()
		{
			throw new NotImplementedException();
		}

		// POST /api/auth/register
		[HttpPost("register")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.RegisterAsync(dto);

			if (!result.IsSuccess)
			{
				if (result.ErrorMessage == "account_taken")
					return Conflict(new { message = "帳號已被使用，請換一個", errorCode = "account_taken" });

				if (result.ErrorMessage == "email_taken")
					return Conflict(new { message = "此 Email 已有帳號，請直接登入", errorCode = "email_taken" });

				return BadRequest(new { message = result.ErrorMessage });
			}

			return Ok(new { message = "驗證信已寄出，請至信箱點擊驗證連結" });
		}

		[HttpPost("forgot-password")]
		[EnableRateLimiting("AuthPolicy")]
		public IActionResult ForgotPassword()
		{
			throw new NotImplementedException();
		}

		[HttpPost("resend-verify-email")]
		[EnableRateLimiting("AuthPolicy")]
		public IActionResult ResendVerifyEmail()
		{
			throw new NotImplementedException();
		}
	}
}