using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;

namespace RecruitAI.Application.Commands.Admin
{
	public class DeleteUserCommand : IRequest<AdminUserDeleteResponseDto>
	{
		public Guid UserId { get; set; }
		public Guid CurrentAdminId { get; set; }
	}
}