using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Interfaces.Repositories;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginationResponseDto<AuditLogResponseDto>>
	{
		private readonly IAuditLogRepository _auditLogRepository;
		private readonly IMessageService _msg;

		public GetAuditLogsQueryHandler(
			IAuditLogRepository auditLogRepository,
			IMessageService msg)
		{
			_auditLogRepository = auditLogRepository;
			_msg = msg;
		}

		public async Task<PaginationResponseDto<AuditLogResponseDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
		{
			var result = await _auditLogRepository.GetAuditLogsAsync(
				request.Page,
				request.PageSize,
				request.EntityType,
				request.Action,
				request.UserId,
				request.FromDate,
				request.ToDate,
				request.Keyword,
				request.SortBy,
				request.SortOrder,
				cancellationToken);

			var items = result.Items.Select(log => new AuditLogResponseDto
			{
				Id = log.Id,
				EntityType = AuditLogHelper.GetEntityTypeName(log.EntityType, _msg),  
				Action = AuditLogHelper.GetActionName(log.Action, _msg),              
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