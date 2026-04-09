using RecruitAI.Application.DTOs.Common;

namespace RecruitAI.Application.DTOs.Requests.Admin
{
	public class GetAuditLogsRequestDto : PaginationRequestDto
	{
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