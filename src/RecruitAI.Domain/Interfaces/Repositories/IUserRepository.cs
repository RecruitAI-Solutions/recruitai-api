using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RecruitAI.Domain.Interfaces.Repositories
{
	public interface IUserRepository : IBaseRepository<User>
	{
		Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
		Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
		Task<User> GetUserWithAuthProvidersAsync(Guid userId, CancellationToken cancellationToken = default);
		Task<User> GetUserWithRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
		Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
		Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
		Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);
		Task UpdateUserStatusAsync(Guid userId, UserStatus status, CancellationToken cancellationToken = default);
		new Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
		new Task<bool> AnyAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);
		Task<PagedResult<User>> GetUsersAsync(
			int page,
			int pageSize,
			UserRole? role,
			UserStatus? status,
			string? keyword,
			string sortBy,
			string sortOrder,
			CancellationToken cancellationToken = default);

		Task<User?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
		Task<bool> SoftDeleteAsync(Guid userId, CancellationToken cancellationToken = default);
		Task<Dictionary<UserRole, int>> CountUsersByRoleAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
		IQueryable<User> GetQueryable();
	}
}

