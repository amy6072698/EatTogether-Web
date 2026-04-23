namespace EatTogether.API.Models.DTOs
{
    public class MemberLoginDto
    {
        public int MemberId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
