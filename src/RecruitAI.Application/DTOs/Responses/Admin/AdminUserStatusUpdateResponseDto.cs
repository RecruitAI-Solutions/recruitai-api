using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Admin
{
	public class AdminUserStatusUpdateResponseDto
	{
		public Guid Id { get; set; }
		public UserStatus Status { get; set; }
		public string? Reason { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}