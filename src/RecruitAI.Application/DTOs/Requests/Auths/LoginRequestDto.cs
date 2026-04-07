namespace RecruitAI.Application.DTOs.Requests.Auths
{
	public class LoginRequestDto
	{
		public string Email { get; set; }

		public string Password { get; set; }
		public string? CreatedByIp { get; set; }
	}
}