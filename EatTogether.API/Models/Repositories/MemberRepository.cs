using EatTogether.API.Models.EfModels;
using EatTogether.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.API.Models.Repositories
{
	public interface IMemberRepository
	{
		Task<MemberDetailDto?> GetByIdAsync(int id);
		Task<MemberListDto?> GetByPhoneAsync(string phone);
		// -----內用點餐頁用------------------------------
		Task<Member?> GetByEmailAsync(string email);

		// -----前台會員註冊用------------------------------
		Task<bool> IsAccountExistsAsync(string account);
		Task<bool> IsEmailExistsAsync(string email);
		Task CreateMemberAsync(Member member);
		Task<bool> HasPendingConfirmTokenAsync(int memberId);
		Task CreateConfirmTokenAsync(MemberConfirmToken token);

		// -----前台一般登入用------------------------------
		Task<Member?> GetMemberByAccountAsync(string account);
		Task SaveRefreshTokenAsync(MemberRefreshToken token);

		// -----前台復原帳號用------------------------------
		Task RestoreAccountAsync(int memberId);

		// -----前台 Email 驗證用------------------------------
		Task<MemberConfirmToken?> GetConfirmTokenAsync(string token);
		Task MarkConfirmTokenUsedAsync(int tokenId);
		Task SetMemberConfirmedAsync(int memberId);
		Task UpdateMemberEmailAsync(int memberId, string newEmail);
		Task ConfirmMemberAndMarkTokenUsedAsync(int memberId, int tokenId);
		Task UpdateEmailAndMarkTokenUsedAsync(int memberId, string newEmail, int tokenId);
		Task MarkOldConfirmTokensUsedAsync(int memberId);
		Task<Member?> GetMemberByIdAsync(int id);
	}

	public class MemberRepository : IMemberRepository
	{
		private readonly EatTogetherDBContext _context;

		public MemberRepository(EatTogetherDBContext context)
		{
			_context = context;
		}

		// 取單筆詳情
		public async Task<MemberDetailDto?> GetByIdAsync(int id)
		{
			return await _context.Members
				.AsNoTracking()
				.Where(m => m.Id == id)
				.Select(m => new MemberDetailDto
				{
					Id = m.Id,
					Name = m.Name,
					Account = m.Account,
					Email = m.Email,
					Phone = m.Phone,
					BirthDate = m.BirthDate,
					CreatedAt = m.CreatedAt,
					IsConfirmed = m.IsConfirmed,
					IsBlacklisted = m.IsBlacklisted,
					IsDeleted = m.IsDeleted,
					DeletedAt = m.DeletedAt,
					BlacklistReason = m.BlacklistReason,
					AvatarFileName = m.AvatarFileName,
				})
				.FirstOrDefaultAsync();
		}

		// 以電話號碼查單筆會員
		public async Task<MemberListDto?> GetByPhoneAsync(string phone)
		{
			var trimmed = phone.Trim();
			return await _context.Members
				.AsNoTracking()
				.Where(m => m.Phone == trimmed && !m.IsDeleted)
				.Select(m => new MemberListDto
				{
					Id = m.Id,
					Name = m.Name,
					Account = m.Account,
					Email = m.Email,
					Phone = m.Phone,
					BirthDate = m.BirthDate,
					CreatedAt = m.CreatedAt,
					IsConfirmed = m.IsConfirmed,
					IsBlacklisted = m.IsBlacklisted,
					IsDeleted = m.IsDeleted,
					DeletedAt = m.DeletedAt,
					BlacklistReason = m.BlacklistReason,
				})
				.FirstOrDefaultAsync();
		}


		// -----前台點餐頁用-----
		public async Task<Member?> GetByEmailAsync(string email)
		{
			return await _context.Members
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.Email == email && !m.IsDeleted);
		}

		// -----前台會員註冊用------------------------------

		// 帳號唯一性檢查（含軟刪除會員，避免違反 Unique Index）
		public async Task<bool> IsAccountExistsAsync(string account)
		{
			return await _context.Members.AnyAsync(m => m.Account == account);
		}

		// Email 唯一性檢查（含軟刪除會員，避免違反 Unique Index）
		public async Task<bool> IsEmailExistsAsync(string email)
		{
			return await _context.Members.AnyAsync(m => m.Email == email);
		}

		public async Task CreateMemberAsync(Member member)
		{
			_context.Members.Add(member);
			await _context.SaveChangesAsync();
		}

		// 寄信保護：查詢是否已有未過期且未使用的註冊驗證 token
		public async Task<bool> HasPendingConfirmTokenAsync(int memberId)
		{
			return await _context.MemberConfirmTokens.AnyAsync(t =>
				t.MemberId == memberId &&
				!t.IsUsed &&
				t.ExpiresAt > DateTime.Now &&
				t.NewEmail == null);
		}

		public async Task CreateConfirmTokenAsync(MemberConfirmToken token)
		{
			_context.MemberConfirmTokens.Add(token);
			await _context.SaveChangesAsync();
		}

		// -----前台 Email 驗證用------------------------------

		public async Task<MemberConfirmToken?> GetConfirmTokenAsync(string token)
		{
			return await _context.MemberConfirmTokens
				.FirstOrDefaultAsync(t => t.Token == token);
		}

		public async Task MarkConfirmTokenUsedAsync(int tokenId)
		{
			var token = await _context.MemberConfirmTokens.FindAsync(tokenId);
			if (token == null) return;
			token.IsUsed = true;
			await _context.SaveChangesAsync();
		}

		public async Task SetMemberConfirmedAsync(int memberId)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.IsConfirmed = true;
			await _context.SaveChangesAsync();
		}

		public async Task UpdateMemberEmailAsync(int memberId, string newEmail)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.Email = newEmail;
			await _context.SaveChangesAsync();
		}

		public async Task ConfirmMemberAndMarkTokenUsedAsync(int memberId, int tokenId)
		{
			var member = await _context.Members.FindAsync(memberId);
			var token = await _context.MemberConfirmTokens.FindAsync(tokenId);
			if (member == null || token == null) return;

			member.IsConfirmed = true;
			token.IsUsed = true;
			await _context.SaveChangesAsync(); // 兩個變更一次提交，EF Core 自動包隱式 Transaction
		}

		public async Task UpdateEmailAndMarkTokenUsedAsync(int memberId, string newEmail, int tokenId)
		{
			var member = await _context.Members.FindAsync(memberId);
			var token = await _context.MemberConfirmTokens.FindAsync(tokenId);
			if (member == null || token == null) return;

			member.Email = newEmail;
			token.IsUsed = true;
			await _context.SaveChangesAsync();
		}

		public async Task MarkOldConfirmTokensUsedAsync(int memberId)
		{
			var tokens = await _context.MemberConfirmTokens
				.Where(t => t.MemberId == memberId && !t.IsUsed && t.NewEmail == null)
				.ToListAsync();
			if (tokens.Count == 0) return;
			foreach (var t in tokens)
				t.IsUsed = true;
			await _context.SaveChangesAsync();
		}

		public async Task<Member?> GetMemberByIdAsync(int id)
		{
			return await _context.Members.FindAsync(id);
		}

		// -----前台一般登入用------------------------------

		public async Task<Member?> GetMemberByAccountAsync(string account)
		{
			return await _context.Members
				.Include(m => m.MemberExternalLogins)
				.FirstOrDefaultAsync(m => m.Account == account);
		}

		public async Task SaveRefreshTokenAsync(MemberRefreshToken token)
		{
			_context.MemberRefreshTokens.Add(token);
			await _context.SaveChangesAsync();
		}

		// -----前台復原帳號用------------------------------

		public async Task RestoreAccountAsync(int memberId)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.IsDeleted = false;
			member.DeletedAt = null;
			await _context.SaveChangesAsync();
		}
	}
}
