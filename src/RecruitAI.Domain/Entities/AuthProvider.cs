// RecruitAI.Domain/Entities/AuthProvider.cs
using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Entities
{
	public class AuthProvider
	{
		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public AuthProviderType Provider { get; set; }

		public string ProviderUserId { get; set; }

		public string? ProviderEmail { get; set; }

		public string? PasswordHash { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? LastLoginAt { get; set; }

		// Navigation
		public virtual User User { get; set; }
	}
}