using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Interfaces.Services
{
	public interface IAuditLogService
	{
		Task LogAsync(
			AuditEntityType entityType,  
			AuditAction action,
			string entityId,
			string entityName,
			string? oldValue = null,
			string? newValue = null,
			string? reason = null,
			CancellationToken cancellationToken = default);
	}
}