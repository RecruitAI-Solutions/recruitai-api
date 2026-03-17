using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Auths
{
	public class UserProfileDto
	{
		public Guid UserId { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }
		// Roles/permissions
		public int Role { get; set; } 
		public string RoleName { get; set; } = string.Empty;
		public List<string> Permissions { get; set; } = new(); 
		public Gender? Gender { get; set; }
		public string? PhoneNumber { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public UserStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
		public string? AvatarUrl { get; set; }
	}
}
