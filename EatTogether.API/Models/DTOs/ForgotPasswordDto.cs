using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class ForgotPasswordDto
	{
		[Required]
		[EmailAddress]
		[StringLength(100)]
		public string Email { get; set; } = "";
	}
}
