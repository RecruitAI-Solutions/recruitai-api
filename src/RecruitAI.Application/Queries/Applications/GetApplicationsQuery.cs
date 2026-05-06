using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecruitAI.Application.Queries.Applications
{
	public class GetApplicationsQuery : IRequest<PaginationResponseDto<AdminApplicationDto>>
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public JobApplicationStatus? Status { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string? Query { get; set; } // Tìm theo job title, candidate name, email
		public int? MinMatch { get; set; }
		public string? SortBy { get; set; } = "appliedAt";
		public string? SortOrder { get; set; } = "desc";
	}

}
