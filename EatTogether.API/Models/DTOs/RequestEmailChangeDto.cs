using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class RequestEmailChangeDto
	{
		[Required(ErrorMessage = "請輸入新 Email")]
		[EmailAddress(ErrorMessage = "Email 格式不正確")]
		[MaxLength(100, ErrorMessage = "Email 最多 100 字元")]
		public string NewEmail { get; set; } = "";
	}
}
