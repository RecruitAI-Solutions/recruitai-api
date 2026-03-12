namespace RecruitAI.Domain.Entities
{
	public class PasswordResetToken
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Token { get; set; } = string.Empty;
		public DateTime ExpiryDate { get; set; }
		public bool IsUsed { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UsedAt { get; set; }
		public string? CreatedByIp { get; set; }

		// Navigation property
		public virtual User User { get; set; } = null!;
	}
}