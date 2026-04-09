using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Requests.Auths
{
	public class UpdateProfileRequestDto
	{
		public string? FullName { get; set; }
		public string? PhoneNumber { get; set; }
		public Gender? Gender { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? AvatarUrl { get; set; }
	}
}