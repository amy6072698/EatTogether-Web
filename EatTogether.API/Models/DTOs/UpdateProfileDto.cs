using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	public class UpdateProfileDto
	{
		[Required(ErrorMessage = "姓名為必填")]
		[MaxLength(50, ErrorMessage = "姓名最多 50 字元")]
		public string Name { get; set; } = "";

		// 選填；清空時傳 null，不接受空字串
		[MaxLength(10, ErrorMessage = "手機號碼最多 10 碼")]
		[RegularExpression(@"^\d{10}$", ErrorMessage = "手機號碼格式不正確（請填 10 位數字）")]
		public string? Phone { get; set; }

		// 選填；不可為未來日期（由 Service 層驗證）
		public DateOnly? BirthDate { get; set; }
	}
}
