using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Infra;
using EatTogether.Models.Infra;
using EatTogether.Models.Repositories;

namespace EatTogether.Models.Services
{
	public interface IAuthService
	{
		// -----內用點餐頁用------------------------------
		Task<Result<MemberLoginDto>> MemberLoginAsync(string email, string password);

		// -----前台會員註冊用------------------------------
		Task<Result> RegisterAsync(RegisterDto dto);
	}

	public class AuthService : IAuthService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		// -----內用點餐頁用------------------------------
		private readonly IMemberRepository _memberRepo;

		// -----前台會員註冊用------------------------------
		private readonly IEmailService _memberEmailService;
		private readonly IConfiguration _configuration;

		public AuthService(
			IHttpContextAccessor httpContextAccessor,
			IMemberRepository memberRepo,
			IEmailService memberEmailService,
			IConfiguration configuration)
		{
			_httpContextAccessor = httpContextAccessor;
			_memberRepo = memberRepo;
			_memberEmailService = memberEmailService;
			_configuration = configuration;
		}

		// -----前台會員註冊用------------------------------
		public async Task<Result> RegisterAsync(RegisterDto dto)
		{
			// 1. 帳號唯一性檢查
			if (await _memberRepo.IsAccountExistsAsync(dto.Account))
				return Result.Fail("account_taken");

			// 2. Email 唯一性檢查
			if (await _memberRepo.IsEmailExistsAsync(dto.Email))
				return Result.Fail("email_taken");

			// 3. BCrypt Hash
			var hashedPassword = HashUtility.HashPassword(dto.Password);

			// 4. 建立 Member（IsConfirmed=0，待 Email 驗證後開通）
			var member = new Member
			{
				Account = dto.Account,
				Name = dto.Name,
				Email = dto.Email,
				HashedPassword = hashedPassword,
				IsConfirmed = false,
				IsBlacklisted = false,
				IsDeleted = false,
				CreatedAt = DateTime.Now
			};
			await _memberRepo.CreateMemberAsync(member);
			// SaveChangesAsync 後，EF Core 會將自動產生的 Id 回填至 member.Id

			// 5. 寄信保護：同一 MemberId 若已有未過期且未使用的 token，不重建
			if (await _memberRepo.HasPendingConfirmTokenAsync(member.Id))
				return Result.Fail("pending_token");

			// 6. 建立 MemberConfirmToken（有效期 15 分鐘）
			var tokenString = Guid.NewGuid().ToString("N");
			var confirmToken = new MemberConfirmToken
			{
				MemberId = member.Id,
				Token = tokenString,
				NewEmail = null,
				ExpiresAt = DateTime.Now.AddMinutes(15),
				IsUsed = false,
				CreatedAt = DateTime.Now
			};
			await _memberRepo.CreateConfirmTokenAsync(confirmToken);

			// 7. 寄出 Email 驗證信
			var frontendBaseUrl = _configuration["FrontendBaseUrl"];
			var verifyUrl = $"{frontendBaseUrl}/verify-email?token={tokenString}";
			await _memberEmailService.SendVerifyEmailAsync(dto.Email, verifyUrl);

			return Result.Success();
		}

		// -----內用點餐頁用------------------------------
		public async Task<Result<MemberLoginDto>> MemberLoginAsync(string email, string password)
		{
			var member = await _memberRepo.GetByEmailAsync(email);

			if (member == null)
				return Result<MemberLoginDto>.Fail($"找不到email: {email}");

			if (!HashUtility.VerifyPassword(password, member.HashedPassword))
				return Result<MemberLoginDto>.Fail($"密碼錯誤");

			if (member.IsBlacklisted)
				return Result<MemberLoginDto>.Fail("黑名單");

			if (!member.IsConfirmed)
				return Result<MemberLoginDto>.Fail("未驗證");

			return Result<MemberLoginDto>.Success(new MemberLoginDto
			{
				MemberId = member.Id,
				Name = member.Name,
				Email = member.Email,
			});
		}
	}
}
