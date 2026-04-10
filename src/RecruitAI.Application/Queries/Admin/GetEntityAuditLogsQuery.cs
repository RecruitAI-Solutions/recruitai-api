using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetEntityAuditLogsQuery : IRequest<PaginationResponseDto<AuditLogResponseDto>>
	{
		public AuditEntityType EntityType { get; set; } 
		public string EntityId { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;
	}
}