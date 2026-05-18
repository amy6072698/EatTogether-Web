using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class CreateAccountDto
	{
		[Required(ErrorMessage = "請輸入帳號")]
		[RegularExpression(@"^[a-zA-Z0-9_]{3,50}$", ErrorMessage = "帳號限英數字及底線，長度 3–50 字元")]
		public string Account { get; set; } = "";

		[Required(ErrorMessage = "請輸入密碼")]
		[MinLength(8, ErrorMessage = "密碼最少 8 個字元")]
		[MaxLength(128, ErrorMessage = "密碼最多 128 個字元")]
		public string Password { get; set; } = "";
	}
}
