using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Commands.Admin
{
	public class UpdateUserRoleCommand : IRequest<AdminUserRoleUpdateResponseDto>
	{
		public Guid UserId { get; set; }
		public UserRole Role { get; set; }
	}
}