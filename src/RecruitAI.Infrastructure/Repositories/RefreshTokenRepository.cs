using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.Interfaces.Repositories;
using RecruitAI.Domain.Entities;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
	public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
	{
		public RefreshTokenRepository(RecruitDevContext context) : base(context)
		{
		}

		public async Task<RefreshToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(rt => rt.User)
				.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
		}

		public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpireAt > DateTime.UtcNow)
				.OrderByDescending(rt => rt.CreatedAt)
				.ToListAsync(cancellationToken);
		}

		public async Task RevokeTokenAsync(
			string token,
			string revokedByIp,
			string replacedByToken = null,
			CancellationToken cancellationToken = default)
		{
			var refreshToken = await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
			if (refreshToken != null && !refreshToken.IsRevoked)
			{
				refreshToken.IsRevoked = true;
				refreshToken.RevokedAt = DateTime.UtcNow;
				refreshToken.RevokedByIp = revokedByIp;
				refreshToken.ReplacedByToken = replacedByToken;
				Update(refreshToken);
				// Không SaveChanges ở đây vì UnitOfWork sẽ handle
			}
		}

		public async Task RevokeAllUserTokensAsync(
			Guid userId,
			string revokedByIp,
			string exceptToken = null,
			CancellationToken cancellationToken = default)
		{
			var query = _dbSet.Where(rt => rt.UserId == userId && !rt.IsRevoked);

			if (!string.IsNullOrEmpty(exceptToken))
			{
				query = query.Where(rt => rt.Token != exceptToken);
			}

			var tokens = await query.ToListAsync(cancellationToken);
			foreach (var token in tokens)
			{
				token.IsRevoked = true;
				token.RevokedAt = DateTime.UtcNow;
				token.RevokedByIp = revokedByIp;
			}
			// Không SaveChanges ở đây vì UnitOfWork sẽ handle
		}

		public async Task<bool> IsTokenActiveAsync(string token, CancellationToken cancellationToken = default)
		{
			return await _dbSet.AnyAsync(rt =>
				rt.Token == token &&
				!rt.IsRevoked &&
				rt.ExpireAt > DateTime.UtcNow, cancellationToken);
		}

		public async Task<int> GetActiveTokensCountAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			return await _dbSet.CountAsync(rt =>
				rt.UserId == userId &&
				!rt.IsRevoked &&
				rt.ExpireAt > DateTime.UtcNow, cancellationToken);
		}

		public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
		{
			var expiredTokens = await _dbSet
				.Where(rt => rt.ExpireAt <= DateTime.UtcNow || rt.IsRevoked)
				.ToListAsync(cancellationToken);

			if (expiredTokens.Any())
			{
				_dbSet.RemoveRange(expiredTokens);
				// Không SaveChanges ở đây vì UnitOfWork sẽ handle
			}
		}

		// Optional: Thêm method để lấy token còn hạn
		public async Task<RefreshToken> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(rt => rt.User)
				.FirstOrDefaultAsync(rt =>
					rt.Token == token &&
					!rt.IsRevoked &&
					rt.ExpireAt > DateTime.UtcNow,
					cancellationToken);
		}

		// Optional: Revoke token bằng Id
		public async Task RevokeTokenByIdAsync(Guid id, string revokedByIp, CancellationToken cancellationToken = default)
		{
			var refreshToken = await GetByIdAsync(id, cancellationToken);
			if (refreshToken != null && !refreshToken.IsRevoked)
			{
				refreshToken.IsRevoked = true;
				refreshToken.RevokedAt = DateTime.UtcNow;
				refreshToken.RevokedByIp = revokedByIp;
				Update(refreshToken);
			}
		}
	}
}