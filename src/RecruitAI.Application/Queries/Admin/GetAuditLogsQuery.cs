using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Admin;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetAuditLogsQuery : IRequest<PaginationResponseDto<AuditLogResponseDto>>
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;
		public string? EntityType { get; set; }
		public string? Action { get; set; }
		public Guid? UserId { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string? Keyword { get; set; }
		public string SortBy { get; set; } = "changedAt";
		public string SortOrder { get; set; } = "desc";
	}
}