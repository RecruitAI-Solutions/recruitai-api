using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Commands.Admin
{
	public class UpdateUserStatusCommand : IRequest<AdminUserStatusUpdateResponseDto>
	{
		public Guid UserId { get; set; }
		public UserStatus Status { get; set; }
		public string? Reason { get; set; }
	}
}