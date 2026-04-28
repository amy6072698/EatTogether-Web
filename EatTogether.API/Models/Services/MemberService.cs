using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Infra;
using EatTogether.API.Models.Repositories;
using EatTogether.API.Models.ViewModels;

namespace EatTogether.API.Models.Services
{
	public interface IMemberService
	{
		Task<Result<MemberViewModel>> GetMeAsync(int memberId);
		Task<Result> UpdateProfileAsync(int memberId, UpdateProfileDto dto);
		Task<Result<string>> UploadAvatarAsync(int memberId, IFormFile file);
		Task<Result> ChangePasswordAsync(int memberId, ChangePasswordDto dto);
		Task<Result> RequestEmailChangeAsync(int memberId, RequestEmailChangeDto dto);
		Task<Result> CreateAccountAsync(int memberId, CreateAccountDto dto);
		Task<Result> UnlinkGoogleAsync(int memberId);
		Task<Result> DeleteAccountAsync(int memberId, DeleteAccountDto dto);
	}

	public class MemberService : IMemberService
	{
		private readonly IMemberRepository _memberRepo;
		private readonly IEmailService _emailService;
		private readonly IConfiguration _config;
		private readonly IWebHostEnvironment _env;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public MemberService(
			IMemberRepository memberRepo,
			IEmailService emailService,
			IConfiguration config,
			IWebHostEnvironment env,
			IHttpContextAccessor httpContextAccessor)
		{
			_memberRepo = memberRepo;
			_emailService = emailService;
			_config = config;
			_env = env;
			_httpContextAccessor = httpContextAccessor;
		}

		// GET /api/members/me
		public async Task<Result<MemberViewModel>> GetMeAsync(int memberId)
		{
			var member = await _memberRepo.GetMemberByIdAsync(memberId);
			if (member == null || member.IsDeleted)
				return Result<MemberViewModel>.Fail("member_not_found");

			var googleLogin = await _memberRepo.GetExternalLoginByMemberIdAsync(memberId, "google");

			return Result<MemberViewModel>.Success(new MemberViewModel
			{
				Id = member.Id,
				Name = member.Name,
				Email = member.Email,
				Phone = member.Phone,
				BirthDate = member.BirthDate,
				AvatarFileName = member.AvatarFileName,
				GoogleAvatarUrl = googleLogin?.AvatarUrl,
				HashedPasswordStatus = member.HashedPassword == HashUtility.EXTERNAL_LOGIN_NO_PASSWORD
					? "EXTERNAL_LOGIN_NO_PASSWORD"
					: "HAS_PASSWORD",
				GoogleLinked = googleLogin != null,
			});
		}

		// PUT /api/members/me/profile
		public async Task<Result> UpdateProfileAsync(int memberId, UpdateProfileDto dto)
		{
			var member = await _memberRepo.GetMemberByIdAsync(memberId);
			if (member == null || member.IsDeleted)
				return Result.Fail("member_not_found");

			// 生日不可為未來日期
			if (dto.BirthDate.HasValue && dto.BirthDate.Value > DateOnly.FromDateTime(DateTime.Today))
				return Result.Fail("invalid_birth_date");

			await _memberRepo.UpdateProfileAsync(memberId, dto.Name, dto.Phone, dto.BirthDate);
			return Result.Success();
		}

		// POST /api/members/me/avatar
		public async Task<Result<string>> UploadAvatarAsync(int memberId, IFormFile file)
		{
			// 1. 驗證格式（ContentType 正規化為小寫比對）
			var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
			if (!allowedTypes.Contains(file.ContentType.ToLower()))
				return Result<string>.Fail("invalid_format");

			// 2. 驗證大小（2MB）
			if (file.Length > 2 * 1024 * 1024)
				return Result<string>.Fail("file_too_large");

			// 3. 取得上傳目錄（優先 StaticFilesRoot，否則使用 WebRootPath）
			var staticRoot = _config["StaticFilesRoot"];
			var baseDir = !string.IsNullOrEmpty(staticRoot) && Directory.Exists(staticRoot)
				? staticRoot
				: _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
			var uploadDir = Path.Combine(baseDir, "uploads", "avatars");
			Directory.CreateDirectory(uploadDir); // 不存在時自動建立

			// 4. 查詢會員（確認存在 + 取得舊檔名）
			var member = await _memberRepo.GetMemberByIdAsync(memberId);
			if (member == null || member.IsDeleted)
				return Result<string>.Fail("member_not_found");

			// 5. 刪除舊檔案（若有）
			if (!string.IsNullOrEmpty(member.AvatarFileName))
			{
				var oldPath = Path.Combine(uploadDir, member.AvatarFileName);
				if (File.Exists(oldPath))
					File.Delete(oldPath);
			}

			// 6. 產生唯一檔名並存檔
			var ext = file.ContentType.ToLower() switch
			{
				"image/jpeg" => ".jpg",
				"image/png" => ".png",
				"image/webp" => ".webp",
				_ => ".jpg",
			};
			var newFileName = $"{Guid.NewGuid():N}{ext}";
			var newFilePath = Path.Combine(uploadDir, newFileName);

			using (var stream = new FileStream(newFilePath, FileMode.Create))
				await file.CopyToAsync(stream);

			// 7. 更新 DB
			await _memberRepo.UpdateAvatarAsync(memberId, newFileName);

			return Result<string>.Success(newFileName);
		}

		// PUT /api/members/me/password
		public async Task<Result> ChangePasswordAsync(int memberId, ChangePasswordDto dto)
		{
			var member = await _memberRepo.GetMemberByIdAsync(memberId);
			if (member == null || member.IsDeleted)
				return Result.Fail("member_not_found");

			// 純 Google 帳號沒有密碼可以修改
			if (member.HashedPassword == HashUtility.EXTERNAL_LOGIN_NO_PASSWORD)
				return Result.Fail("no_password");

			// 驗證目前密碼
			if (!HashUtility.VerifyPassword(dto.CurrentPassword, member.HashedPassword))
				return Result.Fail("wrong_password");

			var hashed = HashUtility.HashPassword(dto.NewPassword);
			await _memberRepo.UpdateMemberPasswordAsync(memberId, hashed);
			return Result.Success();
		}

		// POST /api/members/me/email
		public async Task<Result> RequestEmailChangeAsync(int memberId, RequestEmailChangeDto dto)
		{
			var member = await _memberRepo.GetMemberByIdAsync(memberId);
			if (member == null || member.IsDeleted)
				return Result.Fail("member_not_found");

			// 不可與目前 Email 相同
			if (member.Email.Equals(dto.NewEmail, StringComparison.OrdinalIgnoreCase))
				return Result.Fail("same_email");

			// 新 Email 是否被他人使用
			if (await _memberRepo.IsEmailExistsForOtherAsync(memberId, dto.NewEmail))
				return Result.Fail("email_taken");

			// 先將同一會員舊的 Email 變更 token 標為已使用，確保同時間只有最新連結有效
			await _memberRepo.MarkOldEmailChangeTokensUsedAsync(memberId);

			// 建立 ConfirmToken（NewEmail 有值 → VerifyEmailAsync 會走 UpdateEmailAndMarkTokenUsedAsync 分支）
			var tokenStr = Guid.NewGuid().ToString("N");
			await _memberRepo.CreateConfirmTokenAsync(new MemberConfirmToken
			{
				MemberId = memberId,
				Token = tokenStr,
				NewEmail = dto.NewEmail,
				ExpiresAt = DateTime.Now.AddMinutes(15),
				IsUsed = false,
				CreatedAt = DateTime.Now,
			});

			// 寄驗證信至新信箱
			var frontendBase = _config["FrontendBaseUrl"];
			var verifyUrl = $"{frontendBase}/verify-email?token={tokenStr}";
			await _emailService.SendEmailChangeVerifyAsync(dto.NewEmail, verifyUrl);

			return Result.Success();
		}

		// POST /api/members/me/create-account
		public async Task<Result> CreateAccountAsync(int memberId, CreateAccountDto dto)
		{
			var member = await _memberRepo.GetMemberByIdAsync(memberId);
			if (member == null || member.IsDeleted)
				return Result.Fail("member_not_found");

			// 只有純 Google 帳號才能建立一般帳號
			if (member.HashedPassword != HashUtility.EXTERNAL_LOGIN_NO_PASSWORD)
				return Result.Fail("already_has_account");

			// 帳號唯一性（全站）
			if (await _memberRepo.IsAccountExistsAsync(dto.Account))
				return Result.Fail("account_taken");

			var hashedPassword = HashUtility.HashPassword(dto.Password);
			await _memberRepo.UpdateAccountAsync(memberId, dto.Account, hashedPassword);
			return Result.Success();
		}

		// DELETE /api/members/me/google-link
		public async Task<Result> UnlinkGoogleAsync(int memberId)
		{
			var member = await _memberRepo.GetMemberByIdAsync(memberId);
			if (member == null || member.IsDeleted)
				return Result.Fail("member_not_found");

			// 純 Google 帳號不得取消連結（需先建立一般帳號）
			if (member.HashedPassword == HashUtility.EXTERNAL_LOGIN_NO_PASSWORD)
				return Result.Fail("cannot_unlink_only_login");

			var googleLogin = await _memberRepo.GetExternalLoginByMemberIdAsync(memberId, "google");
			if (googleLogin == null)
				return Result.Fail("google_not_linked");

			await _memberRepo.DeleteExternalLoginAsync(memberId);
			await _emailService.SendSecurityNoticeAsync(member.Email, "您已取消與 Google 帳號的連結");
			return Result.Success();
		}

		// DELETE /api/members/me
		public async Task<Result> DeleteAccountAsync(int memberId, DeleteAccountDto dto)
		{
			var member = await _memberRepo.GetMemberByIdAsync(memberId);
			if (member == null || member.IsDeleted)
				return Result.Fail("member_not_found");

			// 一般帳號需驗證密碼
			if (member.HashedPassword != HashUtility.EXTERNAL_LOGIN_NO_PASSWORD)
			{
				if (string.IsNullOrEmpty(dto.Password))
					return Result.Fail("password_required");
				if (!HashUtility.VerifyPassword(dto.Password, member.HashedPassword))
					return Result.Fail("wrong_password");
			}

			// 撤銷所有 Refresh Token（踢除所有裝置）
			await _memberRepo.RevokeAllRefreshTokensByMemberIdAsync(memberId);

			// 軟刪除
			await _memberRepo.SoftDeleteMemberAsync(memberId);

			// 清除前端 Cookie
			var response = _httpContextAccessor.HttpContext!.Response;
			CookieHelper.ClearAuthCookies(response, _env);

			return Result.Success();
		}
	}
}
