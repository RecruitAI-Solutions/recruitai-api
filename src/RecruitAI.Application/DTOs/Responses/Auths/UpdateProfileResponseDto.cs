using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Auths
{
	public class UpdateProfileResponseDto
	{
		public Guid UserId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public Gender? Gender { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? AvatarUrl { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}