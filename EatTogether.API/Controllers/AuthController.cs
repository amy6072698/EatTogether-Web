using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.Services;
using EatTogether.API.Models.ViewModels;
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

		// POST /api/auth/login
		[HttpPost("login")]
		[EnableRateLimiting("AuthPolicy")]
		public IActionResult Login()
		{
			return StatusCode(501);
		}

		// POST /api/auth/register
		[HttpPost("register")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// todo Turnstile 驗證
			// var isCaptchaValid = await _captchaService.VerifyAsync(dto.CaptchaToken);
			// if (!isCaptchaValid) return BadRequest(...);

			var result = await _authService.RegisterAsync(dto);

			if (!result.IsSuccess)
			{
				if (result.ErrorMessage == "account_taken")
					return Conflict(new ErrorViewModel
					{
						Message = "帳號已被使用，請換一個",
						ErrorCode = "account_taken"
					});

				if (result.ErrorMessage == "email_taken")
					return Conflict(new ErrorViewModel
					{
						Message = "此 Email 已有帳號，請直接登入",
						ErrorCode = "email_taken"
					});

				return BadRequest(new ErrorViewModel
				{
					Message = result.ErrorMessage ?? "發生錯誤，請稍後再試"
				});
			}

			return Ok(new RegisterResultViewModel
			{
				Message = "驗證信已寄出，請至信箱點擊驗證連結"
			});
		}

		// POST /api/auth/forgot-password
		[HttpPost("forgot-password")]
		[EnableRateLimiting("AuthPolicy")]
		public IActionResult ForgotPassword()
		{
			return StatusCode(501);
		}

		// POST /api/auth/resend-verify-email
		[HttpPost("resend-verify-email")]
		[EnableRateLimiting("AuthPolicy")]
		public IActionResult ResendVerifyEmail()
		{
			return StatusCode(501);
		}
	}
}