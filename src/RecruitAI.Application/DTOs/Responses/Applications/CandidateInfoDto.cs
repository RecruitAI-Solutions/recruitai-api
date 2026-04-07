namespace RecruitAI.Application.DTOs.Responses.Applications
{
	public class CandidateInfoDto
	{
		public Guid Id { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public string? AvatarUrl { get; set; }
	}
}