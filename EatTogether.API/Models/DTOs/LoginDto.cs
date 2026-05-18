using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class LoginDto
	{
		[Required]
		[StringLength(100)]
		public string Account { get; set; } = "";

		[Required]
		[StringLength(128, MinimumLength = 1)]
		public string Password { get; set; } = "";
	}
}
