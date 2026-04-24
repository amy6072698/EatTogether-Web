using EatTogether.API.Models.EfModels;
using EatTogether.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.Models.Repositories
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


		// -----內用點餐頁用------------------------------
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
	}
}
