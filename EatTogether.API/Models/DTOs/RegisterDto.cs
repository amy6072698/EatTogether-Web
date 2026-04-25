using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class RegisterDto
	{
		[Required]
		[StringLength(50, MinimumLength = 3)]
		[RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "帳號限英數字")]
		public string Account { get; set; } = "";

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public string Name { get; set; } = "";

		[Required]
		[EmailAddress]
		[StringLength(100)]
		public string Email { get; set; } = "";

		[Required]
		[StringLength(128, MinimumLength = 8)]
		public string Password { get; set; } = "";

		// 新增這行來接收前端傳來的驗證碼 Token
		public string CaptchaToken { get; set; }
	}
}
