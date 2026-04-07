using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetUserDetailQuery : IRequest<AdminUserDetailResponseDto>
	{
		public Guid UserId { get; set; }
	}
}