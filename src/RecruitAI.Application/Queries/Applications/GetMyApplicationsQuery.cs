using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Queries.Applications;

public class GetMyApplicationsQuery : IRequest<PaginationResponseDto<MyApplicationDto>>
{
	public Guid UserId { get; set; }
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public JobApplicationStatus? Status { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public string? Query { get; set; } // Tìm theo tên job hoặc tên công ty
	public string? SortBy { get; set; } = "appliedAt";
	public string? SortOrder { get; set; } = "desc";

}