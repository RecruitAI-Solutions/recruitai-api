using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Requests.Admin
{
	public class AdminUserStatusUpdateRequestDto
	{
		public UserStatus Status { get; set; }
		public string? Reason { get; set; }
	}
}