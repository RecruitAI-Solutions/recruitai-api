using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetAuditLogDetailQueryHandler : IRequestHandler<GetAuditLogDetailQuery, AuditLogDetailResponseDto>
	{
		private readonly IAuditLogRepository _auditLogRepository;
		private readonly IMessageService _msg;

		public GetAuditLogDetailQueryHandler(
			IAuditLogRepository auditLogRepository,
			IMessageService msg)
		{
			_auditLogRepository = auditLogRepository;
			_msg = msg;
		}

		public async Task<AuditLogDetailResponseDto> Handle(GetAuditLogDetailQuery request, CancellationToken cancellationToken)
		{
			var log = await _auditLogRepository.GetByIdAsync(request.Id, cancellationToken);

			if (log == null)
				throw new BusinessException(ErrorCode.ResourceNotFound, "Audit log not found");

			return new AuditLogDetailResponseDto
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
				UserAgent = log.UserAgent,
				RequestId = log.RequestId,
				ChangedAt = log.ChangedAt
			};
		}
	}
}