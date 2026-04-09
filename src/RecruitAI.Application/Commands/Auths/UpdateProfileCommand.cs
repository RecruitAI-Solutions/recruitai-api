using MediatR;
using RecruitAI.Application.DTOs.Responses.Auths;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Commands.Auths
{
	public class UpdateProfileCommand : IRequest<UpdateProfileResponseDto>
	{
		public Guid UserId { get; set; }
		public string? FullName { get; set; }
		public string? PhoneNumber { get; set; }
		public Gender? Gender { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? AvatarUrl { get; set; }
	}
}