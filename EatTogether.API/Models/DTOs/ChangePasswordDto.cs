using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class ChangePasswordDto
	{
		[Required(ErrorMessage = "請輸入目前密碼")]
		public string CurrentPassword { get; set; } = "";

		[Required(ErrorMessage = "請輸入新密碼")]
		[MinLength(8, ErrorMessage = "密碼最少 8 個字元")]
		[MaxLength(128, ErrorMessage = "密碼最多 128 個字元")]
		public string NewPassword { get; set; } = "";
	}
}
