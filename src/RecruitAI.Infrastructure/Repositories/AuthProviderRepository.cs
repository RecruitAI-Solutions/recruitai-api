using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
	public class AuthProviderRepository : BaseRepository<AuthProvider>, IAuthProviderRepository
	{
		public AuthProviderRepository(RecruitDevContext context) : base(context)
		{
		}

		public async Task<AuthProvider> GetByProviderAndUserIdAsync(
			AuthProviderType provider,
			string providerUserId,
			CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(ap => ap.User)
				.FirstOrDefaultAsync(ap => ap.Provider == provider && ap.ProviderUserId == providerUserId,
					cancellationToken);
		}

		public async Task<AuthProvider> GetLocalAuthByEmailAsync(
			string email,
			CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(ap => ap.User)
				.FirstOrDefaultAsync(ap => ap.Provider == AuthProviderType.Email && ap.ProviderEmail == email,
					cancellationToken);
		}

		public async Task<IEnumerable<AuthProvider>> GetByUserIdAsync(
			Guid userId,
			CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Where(ap => ap.UserId == userId)
				.OrderBy(ap => ap.Provider)
				.ToListAsync(cancellationToken);
		}

		public async Task<bool> ExistsForUserAsync(
			Guid userId,
			AuthProviderType provider,
			CancellationToken cancellationToken = default)
		{
			return await _dbSet.AnyAsync(ap => ap.UserId == userId && ap.Provider == provider,
				cancellationToken);
		}

		public async Task UpdateLastLoginAsync(
			Guid id,
			CancellationToken cancellationToken = default)
		{
			var provider = await GetByIdAsync(id, cancellationToken);
			if (provider != null)
			{
				provider.LastLoginAt = DateTime.UtcNow;
				Update(provider);
			}
		}

		public async Task<AuthProvider> GetByEmailWithUserAsync(
			string email,
			CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(ap => ap.User)
				.FirstOrDefaultAsync(ap => ap.ProviderEmail == email, cancellationToken);
		}

		public async Task<IEnumerable<AuthProvider>> GetAllByUserIdWithUserAsync(
			Guid userId,
			CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(ap => ap.User)
				.Where(ap => ap.UserId == userId)
				.OrderBy(ap => ap.Provider)
				.ToListAsync(cancellationToken);
		}
	}
}