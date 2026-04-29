namespace EatTogether.API.Models.DTOs
{
	/// <summary>
	/// 刪除帳號請求 DTO。
	/// 一般帳號（HAS_PASSWORD）必須提供 Password；
	/// 純 Google 帳號（EXTERNAL_LOGIN_NO_PASSWORD）可傳 null。
	/// </summary>
	public class DeleteAccountDto
	{
		public string? Password { get; set; }
	}
}
