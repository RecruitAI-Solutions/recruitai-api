using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Interfaces.Repositories
{
	public interface IAuditLogRepository : IBaseRepository<AuditLog>
	{
		Task<PagedResult<AuditLog>> GetAuditLogsAsync(
			int page,
			int pageSize,
			string? entityType,
			string? action,
			Guid? userId,
			DateTime? fromDate,
			DateTime? toDate,
			string? keyword,
			string sortBy,
			string sortOrder,
			CancellationToken cancellationToken = default);

		Task<PagedResult<AuditLog>> GetEntityAuditLogsAsync(
			AuditEntityType entityType,
			Guid entityId,
			int page,
			int pageSize,
			CancellationToken cancellationToken = default);
	}
}