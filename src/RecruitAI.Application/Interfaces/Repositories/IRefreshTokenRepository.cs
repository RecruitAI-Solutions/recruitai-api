using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Interfaces.Repositories
{
	public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
	{
		Task<RefreshToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
		Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
		Task RevokeTokenAsync(string token, string revokedByIp, string replacedByToken = null, CancellationToken cancellationToken = default);
		Task RevokeAllUserTokensAsync(Guid userId, string revokedByIp, string exceptToken = null, CancellationToken cancellationToken = default);
		Task<bool> IsTokenActiveAsync(string token, CancellationToken cancellationToken = default);
		Task<int> GetActiveTokensCountAsync(Guid userId, CancellationToken cancellationToken = default);
		Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
	}
}
