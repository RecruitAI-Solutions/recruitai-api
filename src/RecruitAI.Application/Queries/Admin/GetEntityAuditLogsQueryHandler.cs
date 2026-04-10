using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Domain.Interfaces.Repositories;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetEntityAuditLogsQueryHandler : IRequestHandler<GetEntityAuditLogsQuery, PaginationResponseDto<AuditLogResponseDto>>
	{
		private readonly IAuditLogRepository _auditLogRepository;

		public GetEntityAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
		{
			_auditLogRepository = auditLogRepository;
		}

		public async Task<PaginationResponseDto<AuditLogResponseDto>> Handle(GetEntityAuditLogsQuery request, CancellationToken cancellationToken)
		{
			var result = await _auditLogRepository.GetEntityAuditLogsAsync(
				request.EntityType,
				request.EntityId,
				request.Page,
				request.PageSize,
				cancellationToken);

			var items = result.Items.Select(log => new AuditLogResponseDto
			{
				Id = log.Id,
				EntityType = log.EntityType.ToString(), 
				Action = log.Action.ToString(),         
				EntityId = log.EntityId,
				EntityName = log.EntityName,
				OldValue = log.OldValue,
				NewValue = log.NewValue,
				Reason = log.Reason,
				ChangedBy = log.ChangedBy,
				ChangedByIp = log.ChangedByIp,
				ChangedAt = log.ChangedAt
			}).ToList();

			return new PaginationResponseDto<AuditLogResponseDto>
			{
				Data = items,
				Total = result.Total,
				Page = request.Page,
				PageSize = request.PageSize
			};
		}
	}
}