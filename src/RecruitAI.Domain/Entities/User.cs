using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Entities
{
	public class User
	{
		public Guid Id { get; set; }
		public UserRole Role { get; set; } = UserRole.CANDIDATE;
		public string Email { get; set; }

		public string FullName { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		public UserStatus Status { get; set; } = UserStatus.PendingVerification;

		public bool EmailVerified { get; set; } = false;

		public DateTime? LastLoginAt { get; set; }

		public Gender? Gender { get; set; }

		public DateTime? DateOfBirth { get; set; }

		public string? PhoneNumber { get; set; }

		public string? AvatarUrl { get; set; }
		public string? PermissionCodes { get; set; } // Ví dụ: "P001,P002,P003"


		// Navigation properties
		public virtual ICollection<AuthProvider> AuthProviders { get; set; }
		public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
		public virtual ICollection<CV> CVs { get; set; } = new HashSet<CV>();

		// Helper methods
		public List<string> GetPermissionList()
		{
			if (string.IsNullOrEmpty(PermissionCodes))
				return new List<string>();

			return PermissionCodes.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
		}

		public void SetPermissions(List<string> permissions)
		{
			PermissionCodes = string.Join(",", permissions);
		}

		public bool HasPermission(string permissionCode)
		{
			var permissions = GetPermissionList();
			return permissions.Contains(permissionCode) || permissions.Contains("P015");
		}
	}
}
