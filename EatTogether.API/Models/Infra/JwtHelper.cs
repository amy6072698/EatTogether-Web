using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EatTogether.API.Models.Infra
{
	public class JwtHelper
	{
		private readonly string _secretKey;
		private readonly string _issuer;
		private readonly string _audience;
		private readonly int _accessTokenExpireMinutes;

		public JwtHelper(IConfiguration configuration)
		{
			_secretKey = configuration["Jwt:SecretKey"]!;
			_issuer = configuration["Jwt:Issuer"]!;
			_audience = configuration["Jwt:Audience"]!;
			_accessTokenExpireMinutes = int.Parse(configuration["Jwt:AccessTokenExpireMinutes"]!);
		}

		//簽發 JWT（有效期從 appsettings 讀取）
		public string GenerateAccessToken(int memberId, string name)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, memberId.ToString()),
				new Claim(JwtRegisteredClaimNames.Name, name)
			};

			var token = new JwtSecurityToken(
				issuer: _issuer,
				audience: _audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_accessTokenExpireMinutes),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		//產生隨機 Refresh Token（Guid.NewGuid().ToString("N")）
		public string GenerateRefreshToken()
		{
			return Guid.NewGuid().ToString("N");
		}

		//從過期 JWT 中解析 MemberId（Refresh 流程使用）
		public int GetMemberIdFromToken(string token)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = _issuer,
				ValidateAudience = true,
				ValidAudience = _audience,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = key,
				ValidateLifetime = false
			};

			try
			{
				var handler = new JwtSecurityTokenHandler();
				var principal = handler.ValidateToken(token, validationParameters, out _);
				var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
					?? throw new SecurityTokenException("Missing sub claim.");
				return int.Parse(sub);
			}
			catch (Exception ex) when (ex is not SecurityTokenException)
			{
				throw new SecurityTokenException("Invalid token.", ex);
			}
		}
	}
}