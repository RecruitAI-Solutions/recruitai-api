using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Requests.Admin
{
	public class AdminUserUpdateRequestDto
	{
		public string? FullName { get; set; }
		public string? PhoneNumber { get; set; }
		public Gender? Gender { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public UserRole? Role { get; set; }
		public UserStatus? Status { get; set; }
	}
}