using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitAI.Application.DTOs.Responses.Users;

namespace RecruitAI.Application.Commands.Users
{
	public class UploadAvatarCommand : IRequest<UploadAvatarResponseDto>
	{
		public Guid UserId { get; set; }
		public IFormFile Avatar { get; set; } = null!;
	}
}