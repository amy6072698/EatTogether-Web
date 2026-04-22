using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.Infra;
using EatTogether.Models.Infra;
using EatTogether.Models.Repositories;
using EatTogether.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace EatTogether.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderAuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtHelper _jwtHelper;
        private readonly IMemberRepository _memberRepo;

        public OrderAuthController(IAuthService authService, JwtHelper jwtHelper, IMemberRepository memberRepo)
        {
            _authService = authService;
            _jwtHelper = jwtHelper;
            _memberRepo = memberRepo;
        }

        // POST /api/Auth/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Account) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "帳號與密碼為必填" });
            var result = await _authService.LoginAsync(req.Account, req.Password);
            if (!result.IsSuccess)
                return Unauthorized(new { message = result.ErrorMessage ?? "帳號或密碼錯誤" });
            var dto = result.Value!;
            if (dto.MustChangePassword)
                return Unauthorized(new { message = "請先更改密碼後再登入" });

            var token = _jwtHelper.GenerateAccessToken(dto.UserId, dto.Name);
            var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDev,
                SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });
            return Ok(new { name = dto.Name });
        }

        // POST /api/Auth/MemberLogin
        [HttpPost("MemberLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> MemberLogin([FromBody] MemberLoginRequest req)
        {
            var result = await _authService.MemberLoginAsync(req.Email, req.Password);
            if (!result.IsSuccess)
                return Unauthorized(new { message = result.ErrorMessage });

            var token = _jwtHelper.GenerateAccessToken(result.Value!.MemberId, result.Value!.Name);
            var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDev,
                SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });
            return Ok(new { name = result.Value!.Name });
        }

        // GET /api/Auth/Me  — 驗證目前 cookie 是否有效，回傳會員名稱
        [HttpGet("Me")]
        [Authorize]
        public IActionResult Me()
        {
            var name = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value
                    ?? User.Identity?.Name
                    ?? "會員";
            return Ok(new { name });
        }
    }

    public class LoginRequest
    {
        public string Account  { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class MemberLoginRequest
    {
        public string Email    { get; set; } = "";   // 接受 Email 或 Account
        public string Password { get; set; } = "";
    }
}
