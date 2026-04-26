using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class ResendVerifyEmailDto
	{
		[Required]
		[EmailAddress]
		[MaxLength(100)]
		public string Email { get; set; } = "";
	}
}
