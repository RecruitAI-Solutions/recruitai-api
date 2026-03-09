using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Entities
{
	public class AuthProvider
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public AuthProviderType Provider { get; set; } // "email", "facebook", "google"
		public string ProviderUserId { get; set; } // ID từ Facebook/Google
		public string PasswordHash { get; set; } // null nếu login social
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation
		public virtual User User { get; set; }
	}

}
