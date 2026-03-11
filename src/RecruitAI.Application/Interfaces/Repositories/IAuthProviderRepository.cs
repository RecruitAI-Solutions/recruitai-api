using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Interfaces.Repositories
{
	public interface IAuthProviderRepository : IBaseRepository<AuthProvider>
	{
		Task<AuthProvider> GetByProviderAndUserIdAsync(AuthProviderType provider, string providerUserId, CancellationToken cancellationToken = default);
		Task<AuthProvider> GetLocalAuthByEmailAsync(string email, CancellationToken cancellationToken = default);
		Task<IEnumerable<AuthProvider>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
		Task<bool> ExistsForUserAsync(Guid userId, AuthProviderType provider, CancellationToken cancellationToken = default);
		Task UpdateLastLoginAsync(Guid id, CancellationToken cancellationToken = default);
	}
}
