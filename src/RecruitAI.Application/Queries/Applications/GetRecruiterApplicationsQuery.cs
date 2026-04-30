using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Queries.Applications
{
	public class GetRecruiterApplicationsQuery : IRequest<PaginationResponseDto<RecruiterApplicationDto>>
	{
		public Guid RecruiterId { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public JobApplicationStatus? Status { get; set; }
		public Guid? JobId { get; set; }  // Lọc theo job cụ thể
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string? Query { get; set; } // Tìm theo candidate name, email, job title
		public int? MinMatch { get; set; }
		public string? SortBy { get; set; } = "appliedAt";
		public string? SortOrder { get; set; } = "desc";
	}
}
