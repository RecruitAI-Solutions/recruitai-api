using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;
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
	}
}
