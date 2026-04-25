namespace EatTogether.API.Models.ViewModels
{
	// POST /api/auth/register 成功
	public class RegisterResultViewModel
	{
		public string Message { get; set; } = "";
	}

	// 所有錯誤回傳的統一格式
	public class ErrorViewModel
	{
		public string Message { get; set; } = "";
		public string? ErrorCode { get; set; }
	}
}
