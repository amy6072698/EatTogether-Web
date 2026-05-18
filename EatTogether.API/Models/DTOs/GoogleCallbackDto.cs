using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class GoogleCallbackDto
	{
		[Required]
		public string Code { get; set; } = "";
	}
}
