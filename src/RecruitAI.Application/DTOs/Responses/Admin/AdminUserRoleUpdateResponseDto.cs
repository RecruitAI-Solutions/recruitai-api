using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Admin
{
	public class AdminUserRoleUpdateResponseDto
	{
		public Guid Id { get; set; }
		public UserRole Role { get; set; }
		public List<string> Permissions { get; set; } = new();
		public DateTime UpdatedAt { get; set; }
	}
}