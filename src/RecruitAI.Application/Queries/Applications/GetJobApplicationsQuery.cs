using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Queries.Applications;

public class GetJobApplicationsQuery : IRequest<PaginationResponseDto<JobApplicationDto>>
{
	public Guid JobId { get; set; }
	public Guid RecruiterId { get; set; }
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public JobApplicationStatus? Status { get; set; }
	public int? MinMatch { get; set; }
	public string SortBy { get; set; } = "appliedAt";
	public string SortOrder { get; set; } = "desc";
}