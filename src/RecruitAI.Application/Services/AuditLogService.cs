using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Interfaces.Repositories;
using System.Security.Claims;

namespace RecruitAI.Infrastructure.Services
{
	public class AuditLogService : IAuditLogService
	{
		private readonly IAuditLogRepository _auditLogRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<AuditLogService> _logger;

		public AuditLogService(
			IAuditLogRepository auditLogRepository,
			IHttpContextAccessor httpContextAccessor,
			ILogger<AuditLogService> logger,
			IUnitOfWork unitOfWork)
		{
			_auditLogRepository = auditLogRepository;
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task LogAsync(
			AuditEntityType entityType,
			AuditAction action,
			string entityId,
			string entityName,
			string? oldValue = null,
			string? newValue = null,
			string? reason = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var httpContext = _httpContextAccessor.HttpContext;
				var user = httpContext?.User;
				var changedBy = user?.FindFirst(ClaimTypes.Email)?.Value ?? "system";
				var changedByIp = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
				var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
				var requestId = httpContext?.TraceIdentifier;

				var auditLog = new AuditLog
				{
					Id = Guid.NewGuid(),
					EntityType = entityType,
					Action = action,
					EntityId = entityId,
					EntityName = entityName,
					OldValue = oldValue,
					NewValue = newValue,
					Reason = reason,
					ChangedBy = changedBy,
					ChangedByIp = changedByIp,
					UserAgent = userAgent,
					RequestId = requestId,
					ChangedAt = DateTime.UtcNow
				};

				await _auditLogRepository.AddAsync(auditLog, cancellationToken);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				// Log lỗi nhưng không throw để không ảnh hưởng business logic
				_logger.LogError(ex, "Failed to save audit log for {EntityType} {Action}", entityType, action);
			}
		}
	}
}