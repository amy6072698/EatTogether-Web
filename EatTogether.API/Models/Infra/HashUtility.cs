namespace EatTogether.API.Models.Infra
{
	public class HashUtility
	{
		// 純 Google 帳號在資料庫 Members.HashedPassword 欄位儲存此佔位字串，代表「此帳號沒有一般密碼」
		public const string EXTERNAL_LOGIN_NO_PASSWORD = "EXTERNAL_LOGIN_NO_PASSWORD";


		// 呼叫 BCrypt 函式庫的 HashPassword() 方法，回傳雜湊後的密碼字串
		public static string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}


		public static bool VerifyPassword(string password, string hash)
		{
			// 純 Google 帳號、從未設定過密碼，直接回傳 false 拒絕驗證
			if (hash == EXTERNAL_LOGIN_NO_PASSWORD) return false;

			// BCrypt 的 Verify() 方法，將明文密碼與雜湊字串比對
			return BCrypt.Net.BCrypt.Verify(password, hash);
		}
	}
}
