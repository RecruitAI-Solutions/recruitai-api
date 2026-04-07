using MediatR;
using RecruitAI.Application.DTOs.Responses.Applications;

namespace RecruitAI.Application.Queries.Applications
{
	public class GetApplicationDetailQuery : IRequest<ApplicationDetailResponseDto>
	{
		public Guid ApplicationId { get; set; }
		public Guid UserId { get; set; }
	}
}