using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Commands.Admin
{
	public class UpdateUserCommand : IRequest<AdminUserDetailResponseDto>
	{
		public Guid UserId { get; set; }
		public string? FullName { get; set; }
		public string? PhoneNumber { get; set; }
		public Gender? Gender { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public UserRole? Role { get; set; }
		public UserStatus? Status { get; set; }
	}
}