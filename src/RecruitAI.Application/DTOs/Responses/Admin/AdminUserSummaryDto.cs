using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Admin
{
	public class AdminUserSummaryDto
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public UserRole Role { get; set; }
		public UserStatus Status { get; set; }
		public bool EmailVerified { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
		public string? PhoneNumber { get; set; }
	}
}