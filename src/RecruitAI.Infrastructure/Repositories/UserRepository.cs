using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
	public class UserRepository : BaseRepository<User>, IUserRepository
	{
		public UserRepository(RecruitDevContext context) : base(context)
		{
		}

		public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
		}

		public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
		}

		public async Task<User> GetUserWithAuthProvidersAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(u => u.AuthProviders)
				.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
		}

		public async Task<User> GetUserWithRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(u => u.RefreshTokens.Where(rt => !rt.IsRevoked))
				.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
		}

		public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Where(u => u.Role == role && u.Status == UserStatus.Active)
				.OrderBy(u => u.FullName)
				.ToListAsync(cancellationToken);
		}

		public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Where(u => u.Status == UserStatus.Active)
				.OrderBy(u => u.FullName)
				.ToListAsync(cancellationToken);
		}

		public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			var user = await GetByIdAsync(userId, cancellationToken);
			if (user != null)
			{
				user.LastLoginAt = DateTime.UtcNow;
				Update(user);
				// Không SaveChanges ở đây vì UnitOfWork sẽ handle
			}
		}

		public async Task UpdateUserStatusAsync(Guid userId, UserStatus status, CancellationToken cancellationToken = default)
		{
			var user = await GetByIdAsync(userId, cancellationToken);
			if (user != null)
			{
				user.Status = status;
				Update(user);
				// Không SaveChanges ở đây vì UnitOfWork sẽ handle
			}
		}

		// Optional: Thêm method tìm kiếm nâng cao
		public async Task<IEnumerable<User>> SearchUsersAsync(
			string keyword,
			UserRole? role = null,
			UserStatus? status = null,
			CancellationToken cancellationToken = default)
		{
			var query = _dbSet.AsQueryable();

			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(u =>
					u.Email.Contains(keyword) ||
					u.FullName.Contains(keyword) ||
					u.PhoneNumber.Contains(keyword));
			}

			if (role.HasValue)
			{
				query = query.Where(u => u.Role == role.Value);
			}

			if (status.HasValue)
			{
				query = query.Where(u => u.Status == status.Value);
			}

			return await query
				.OrderBy(u => u.FullName)
				.ToListAsync(cancellationToken);
		}

		// Optional: Get user by email với Include tùy chọn
		public async Task<User> GetByEmailWithDetailsAsync(
			string email,
			bool includeAuthProviders = false,
			bool includeRefreshTokens = false,
			CancellationToken cancellationToken = default)
		{
			var query = _dbSet.Where(u => u.Email == email);

			if (includeAuthProviders)
			{
				query = query.Include(u => u.AuthProviders);
			}

			if (includeRefreshTokens)
			{
				query = query.Include(u => u.RefreshTokens.Where(rt => !rt.IsRevoked));
			}

			return await query.FirstOrDefaultAsync(cancellationToken);
		}

		public async Task<PagedResult<User>> GetUsersAsync(
			int page,
			int pageSize,
			UserRole? role,
			UserStatus? status,
			string? keyword,
			string sortBy,
			string sortOrder,
			CancellationToken cancellationToken = default)
		{
			var query = _dbSet.AsQueryable();

			// Filter by role
			if (role.HasValue)
			{
				query = query.Where(u => u.Role == role.Value);
			}

			// Filter by status
			if (status.HasValue)
			{
				query = query.Where(u => u.Status == status.Value);
			}

			// Search by keyword (email, fullname, phone)
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(u =>
					u.Email.Contains(keyword) ||
					u.FullName.Contains(keyword) ||
					(u.PhoneNumber != null && u.PhoneNumber.Contains(keyword)));
			}

			var total = await query.CountAsync(cancellationToken);

			// Sorting
			query = sortBy?.ToLower() switch
			{
				"fullname" => sortOrder == "asc"
					? query.OrderBy(u => u.FullName)
					: query.OrderByDescending(u => u.FullName),
				"email" => sortOrder == "asc"
					? query.OrderBy(u => u.Email)
					: query.OrderByDescending(u => u.Email),
				"status" => sortOrder == "asc"
					? query.OrderBy(u => u.Status)
					: query.OrderByDescending(u => u.Status),
				_ => sortOrder == "asc"
					? query.OrderBy(u => u.CreatedAt)
					: query.OrderByDescending(u => u.CreatedAt)
			};

			var items = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync(cancellationToken);

			return new PagedResult<User>
			{
				Items = items,
				Total = total
			};
		}

		public async Task<User?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
		}

		public async Task<bool> SoftDeleteAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
			if (user == null) return false;

			// Soft delete - mark as deleted
			user.Status = UserStatus.Banned;
			user.UpdatedAt = DateTime.UtcNow;
			_dbSet.Update(user);
			return true;
		}
		public async Task<Dictionary<UserRole, int>> CountUsersByRoleAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
		{
			var query = _dbSet.AsQueryable();

			if (fromDate.HasValue)
				query = query.Where(u => u.CreatedAt >= fromDate.Value);

			if (toDate.HasValue)
			{
				var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
				query = query.Where(u => u.CreatedAt <= toDateEnd);
			}

			var items = await query
				.GroupBy(u => u.Role)
				.Select(g => new { Role = g.Key, Count = g.Count() })
				.ToListAsync(cancellationToken);

			return items.ToDictionary(x => x.Role, x => x.Count);
		}

		public IQueryable<User> GetQueryable()
		{
			return _dbSet.AsQueryable();
		}
	}
}