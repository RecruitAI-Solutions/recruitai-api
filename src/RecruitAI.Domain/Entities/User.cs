using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Entities
{
	public class User
	{
		public Guid Id { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public UserStatus Status { get; set; } = UserStatus.PendingVerification;
		public Gender? Gender { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? PhoneNumber { get; set; }
		public string? AvatarUrl { get; set; }


		// Navigation properties
		public virtual ICollection<AuthProvider> AuthProviders { get; set; }
		public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
	}
}
