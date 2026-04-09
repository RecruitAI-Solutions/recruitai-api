using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
	public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
	{
		private readonly RecruitDevContext _context;

		public AuditLogRepository(RecruitDevContext context) : base(context)
		{
			_context = context;
		}

		public async Task<PagedResult<AuditLog>> GetAuditLogsAsync(
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
			CancellationToken cancellationToken = default)
		{
			var query = _dbSet.AsQueryable();

			if (!string.IsNullOrWhiteSpace(entityType) && Enum.TryParse<AuditEntityType>(entityType, true, out var parsedEntityType))
			{
				query = query.Where(l => l.EntityType == parsedEntityType);
			}

			if (!string.IsNullOrWhiteSpace(action) && Enum.TryParse<AuditAction>(action, true, out var parsedAction))
			{
				query = query.Where(l => l.Action == parsedAction);
			}

			if (userId.HasValue)
				query = query.Where(l => l.ChangedBy == userId.Value.ToString());

			if (fromDate.HasValue)
				query = query.Where(l => l.ChangedAt >= fromDate.Value);

			if (toDate.HasValue)
			{
				var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
				query = query.Where(l => l.ChangedAt <= toDateEnd);
			}

			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(l =>
					l.EntityName.Contains(keyword) ||
					(l.Reason != null && l.Reason.Contains(keyword)));
			}

			var total = await query.CountAsync(cancellationToken);

			// Sorting
			query = sortBy?.ToLower() switch
			{
				"entitytype" => sortOrder == "asc"
					? query.OrderBy(l => l.EntityType)
					: query.OrderByDescending(l => l.EntityType),
				"action" => sortOrder == "asc"
					? query.OrderBy(l => l.Action)
					: query.OrderByDescending(l => l.Action),
				"changedby" => sortOrder == "asc"
					? query.OrderBy(l => l.ChangedBy)
					: query.OrderByDescending(l => l.ChangedBy),
				_ => sortOrder == "asc"
					? query.OrderBy(l => l.ChangedAt)
					: query.OrderByDescending(l => l.ChangedAt)
			};

			var items = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync(cancellationToken);

			return new PagedResult<AuditLog>
			{
				Items = items,
				Total = total
			};
		}

		public async Task<PagedResult<AuditLog>> GetEntityAuditLogsAsync(
			AuditEntityType entityType,
			Guid entityId,
			int page,
			int pageSize,
			CancellationToken cancellationToken = default)
		{
			var query = _dbSet
				.Where(l => l.EntityType == entityType && l.EntityId == entityId)
				.OrderByDescending(l => l.ChangedAt);

			var total = await query.CountAsync(cancellationToken);

			var items = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync(cancellationToken);

			return new PagedResult<AuditLog>
			{
				Items = items,
				Total = total
			};
		}
	}
}