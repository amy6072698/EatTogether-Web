using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.Services;
using EatTogether.API.Models.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EatTogether.API.Controllers
{
	[Route("api/members")]
	[ApiController]
	public class MembersController : ControllerBase
	{
		private readonly IMemberService _memberService;

		public MembersController(IMemberService memberService)
		{
			_memberService = memberService;
		}

		// GET /api/members/me
		[HttpGet("me")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> GetMe()
		{
			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.GetMeAsync(memberId);

			if (!result.IsSuccess)
				return NotFound(new ErrorViewModel
				{
					Message = "找不到會員",
					ErrorCode = result.ErrorMessage
				});

			return Ok(result.Value);
		}

		// PUT /api/members/me/profile
		[HttpPut("me/profile")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.UpdateProfileAsync(memberId, dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"invalid_birth_date" => BadRequest(new ErrorViewModel
					{
						Message = "生日不可為未來日期",
						ErrorCode = "invalid_birth_date"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "更新失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "個人資料已更新" });
		}

		// POST /api/members/me/avatar
		[HttpPost("me/avatar")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		[RequestSizeLimit(3 * 1024 * 1024)] // 3MB 上限，Service 層再驗證 2MB
		public async Task<IActionResult> UploadAvatar(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest(new ErrorViewModel
				{
					Message = "請選擇要上傳的圖片",
					ErrorCode = "no_file"
				});

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.UploadAvatarAsync(memberId, file);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"invalid_format" => BadRequest(new ErrorViewModel
					{
						Message = "僅支援 JPG、PNG、WebP 格式",
						ErrorCode = "invalid_format"
					}),
					"file_too_large" => BadRequest(new ErrorViewModel
					{
						Message = "圖片大小不可超過 2MB",
						ErrorCode = "file_too_large"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "上傳失敗，請稍後再試" })
				};
			}

			return Ok(new { avatarFileName = result.Value });
		}

		// PUT /api/members/me/password
		[HttpPut("me/password")]
		[Authorize]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.ChangePasswordAsync(memberId, dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"no_password" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號使用 Google 登入，無法修改密碼",
						ErrorCode = "no_password"
					}),
					"wrong_password" => BadRequest(new ErrorViewModel
					{
						Message = "目前密碼不正確",
						ErrorCode = "wrong_password"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "修改失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "密碼已成功修改" });
		}

		// POST /api/members/me/email
		[HttpPost("me/email")]
		[Authorize]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.RequestEmailChangeAsync(memberId, dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"same_email" => BadRequest(new ErrorViewModel
					{
						Message = "新 Email 與目前相同",
						ErrorCode = "same_email"
					}),
					"email_taken" => Conflict(new ErrorViewModel
					{
						Message = "此 Email 已被其他帳號使用",
						ErrorCode = "email_taken"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "申請失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "驗證信已寄至新信箱，請點擊連結完成變更" });
		}

		// POST /api/members/me/create-account
		[HttpPost("me/create-account")]
		[Authorize]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.CreateAccountAsync(memberId, dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"already_has_account" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已設有一般帳號密碼",
						ErrorCode = "already_has_account"
					}),
					"account_taken" => Conflict(new ErrorViewModel
					{
						Message = "帳號已被使用，請換一個",
						ErrorCode = "account_taken"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "建立失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "帳號已建立，往後可使用帳號密碼登入" });
		}

		// DELETE /api/members/me/google-link
		[HttpDelete("me/google-link")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> UnlinkGoogle()
		{
			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.UnlinkGoogleAsync(memberId);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"cannot_unlink_only_login" => BadRequest(new ErrorViewModel
					{
						Message = "請先建立帳號密碼後再取消 Google 連結",
						ErrorCode = "cannot_unlink_only_login"
					}),
					"google_not_linked" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號未連結 Google",
						ErrorCode = "google_not_linked"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "取消連結失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "已取消與 Google 帳號的連結" });
		}

		// DELETE /api/members/me
		// EmptyBodyBehavior.Allow：純 Google 帳號刪除時不帶 body，
		// 但 apiFetch.js 仍會送出 Content-Type: application/json，
		// 預設行為會在 Model Binding 階段直接 400，永遠進不到 action
		[HttpDelete("me")]
		[Authorize]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> DeleteAccount(
			[FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DeleteAccountDto? dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.DeleteAccountAsync(memberId, dto ?? new DeleteAccountDto());

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"password_required" => BadRequest(new ErrorViewModel
					{
						Message = "請輸入密碼以確認刪除",
						ErrorCode = "password_required"
					}),
					"wrong_password" => BadRequest(new ErrorViewModel
					{
						Message = "密碼不正確",
						ErrorCode = "wrong_password"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "刪除失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "帳號已刪除" });
		}
	}
}
