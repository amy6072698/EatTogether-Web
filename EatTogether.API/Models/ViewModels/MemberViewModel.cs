namespace EatTogether.API.Models.ViewModels
{
	public class MemberViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string Email { get; set; } = "";
		public string? Phone { get; set; }
		public DateOnly? BirthDate { get; set; }
		public string? AvatarFileName { get; set; }
		public string? GoogleAvatarUrl { get; set; }
		/// <summary>"HAS_PASSWORD" | "EXTERNAL_LOGIN_NO_PASSWORD"</summary>
		public string HashedPasswordStatus { get; set; } = "";
		public bool GoogleLinked { get; set; }
	}
}
