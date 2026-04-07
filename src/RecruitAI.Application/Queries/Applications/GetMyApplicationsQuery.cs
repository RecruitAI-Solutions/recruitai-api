using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Jobs;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Queries.Applications;

public class GetMyApplicationsQuery : IRequest<PaginationResponseDto<MyApplicationDto>>
{
	public Guid UserId { get; set; }
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public JobApplicationStatus? Status { get; set; }
}