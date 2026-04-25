using EatTogether.API.Models.Infra;
using EatTogether.API.Models.Repositories;
using EatTogether.API.Models.Services;
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

        // POST /api/Auth/MemberLogin — 前台會員登入（查 Members 資料表）
        [HttpPost("MemberLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> MemberLogin([FromBody] MemberLoginRequest req)
        {
            var result = await _authService.MemberLoginAsync(req.Email, req.Password);

            if (!result.IsSuccess)
                return Unauthorized(new { message = result.ErrorMessage });

            var token = _jwtHelper.GenerateAccessToken(result.Value!.MemberId, result.Value!.Name);
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
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
