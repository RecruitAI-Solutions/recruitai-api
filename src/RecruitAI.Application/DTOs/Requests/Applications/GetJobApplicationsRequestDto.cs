using RecruitAI.Application.DTOs.Common;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Requests.Applications;

public class GetJobApplicationsRequestDto : PaginationRequestDto
{
	public JobApplicationStatus? Status { get; set; }
	public int? MinMatch { get; set; }
	public string SortBy { get; set; } = "appliedAt";
	public string SortOrder { get; set; } = "desc";
}