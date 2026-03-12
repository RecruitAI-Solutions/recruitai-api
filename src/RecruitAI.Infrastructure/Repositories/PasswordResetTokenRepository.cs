using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.Interfaces.Repositories;
using RecruitAI.Domain.Entities;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
	public class PasswordResetTokenRepository : BaseRepository<PasswordResetToken>, IPasswordResetTokenRepository
	{
		public PasswordResetTokenRepository(RecruitDevContext context) : base(context)
		{
		}

		public async Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(t => t.User)
				.FirstOrDefaultAsync(t => t.Token == token
									   && !t.IsUsed
									   && t.ExpiryDate > DateTime.UtcNow
									   , cancellationToken);
		}

		public async Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			var tokens = await _dbSet
				.Where(t => t.UserId == userId && !t.IsUsed)
				.ToListAsync(cancellationToken);

			foreach (var token in tokens)
			{
				token.IsUsed = true;
				token.UsedAt = DateTime.UtcNow;
			}
		}
	}
}