using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories
{
	public interface IPasswordResetTokenRepository : IBaseRepository<PasswordResetToken>
	{
		Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);
		Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
	}
}