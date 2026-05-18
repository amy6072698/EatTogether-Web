using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class ResetPasswordDto
	{
		[Required]
		[StringLength(200)]
		public string Token { get; set; } = "";

		[Required]
		[StringLength(128, MinimumLength = 8)]
		public string NewPassword { get; set; } = "";
	}
}
