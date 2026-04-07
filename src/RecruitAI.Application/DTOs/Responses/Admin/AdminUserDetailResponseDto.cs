using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Admin
{
	public class AdminUserDetailResponseDto
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public UserRole Role { get; set; }
		public UserStatus Status { get; set; }
		public bool EmailVerified { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
		public Gender? Gender { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? PhoneNumber { get; set; }
		public string? AvatarUrl { get; set; }
		public List<string> Permissions { get; set; } = new();
	}
}