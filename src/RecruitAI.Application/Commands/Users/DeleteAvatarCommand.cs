using MediatR;
using RecruitAI.Application.DTOs.Responses.Users;

namespace RecruitAI.Application.Commands.Users
{
	public class DeleteAvatarCommand : IRequest<DeleteAvatarResponseDto>
	{
		public Guid UserId { get; set; }
	}
}