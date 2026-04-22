using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EatTogether.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		[HttpPost("login")]
		[EnableRateLimiting("AuthPolicy")]
		public IActionResult Login()
		{
			throw new NotImplementedException();
		}

		[HttpPost("register")]
		[EnableRateLimiting("AuthPolicy")]
		public IActionResult Register()
		{
			throw new NotImplementedException();
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