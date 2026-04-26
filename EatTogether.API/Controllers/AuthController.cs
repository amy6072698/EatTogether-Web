using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.Services;
using EatTogether.API.Models.ViewModels.Common;
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
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.LoginAsync(dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"account_or_password_error" => Unauthorized(new ErrorViewModel
					{
						Message = "帳號或密碼錯誤",
						ErrorCode = "account_or_password_error"
					}),
					"EMAIL_NOT_VERIFIED" => Unauthorized(new ErrorViewModel
					{
						Message = "請先驗證 Email",
						ErrorCode = "EMAIL_NOT_VERIFIED"
					}),
					"ACCOUNT_BLACKLISTED" => StatusCode(403, new ErrorViewModel
					{
						Message = "帳號已停權，請聯繫客服",
						ErrorCode = "ACCOUNT_BLACKLISTED"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "發生錯誤，請稍後再試" })
				};
			}

			return Ok(result.Value);
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

			return Ok(new SuccessViewModel
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

		// GET /api/auth/verify-email?token=xxx
		[HttpGet("verify-email")]
		public async Task<IActionResult> VerifyEmail([FromQuery] string token)
		{
			if (string.IsNullOrWhiteSpace(token))
				return BadRequest(new ErrorViewModel
				{
					Message = "驗證連結無效或已過期",
					ErrorCode = "token_invalid"
				});

			var result = await _authService.VerifyEmailAsync(token);
			if (!result.IsSuccess)
				return BadRequest(new ErrorViewModel
				{
					Message = "驗證連結無效或已過期",
					ErrorCode = "token_invalid"
				});

			return Ok(new SuccessViewModel { Message = "Email 驗證成功" });
		}

		// POST /api/auth/resend-verify-email
		[HttpPost("resend-verify-email")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ResendVerifyEmail([FromBody] ResendVerifyEmailDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.ResendVerifyEmailAsync(dto.Email);

			if (!result.IsSuccess && result.ErrorMessage == "already_confirmed")
				return Ok(new ErrorViewModel
				{
					Message = "帳號已完成驗證，請直接登入",
					ErrorCode = "already_confirmed"
				});

			return Ok(new SuccessViewModel { Message = "驗證信已寄出，請至信箱點擊驗證連結" });
		}
	}
}