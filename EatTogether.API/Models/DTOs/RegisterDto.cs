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
		[MinLength(8)]
		public string Password { get; set; } = "";
	}
}
