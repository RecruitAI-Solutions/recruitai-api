using RecruitAI.Domain.Interfaces.Repositories;

namespace RecruitAI.Application.Interfaces
{
	public interface IUnitOfWork : IDisposable
	{
		IUserRepository Users { get; }
		IAuthProviderRepository AuthProviders { get; }
		IRefreshTokenRepository RefreshTokens { get; }
		ITestRepository Tests { get; }
		IPasswordResetTokenRepository PasswordResetTokens { get; }
		IJobRepository Jobs { get; }
		ICVRepository CVs { get; }
		ISkillRepository Skills { get; }
		ICVAnalysisRepository CVAnalysisResults { get; }
		IJobApplicationRepository JobApplications {  get; }
		IJobApplicationMatchRepository JobApplicationMatches {  get; }

		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
		Task BeginTransactionAsync(CancellationToken cancellationToken = default);
		Task CommitTransactionAsync(CancellationToken cancellationToken = default);
		Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
		bool HasActiveTransaction { get; }

		Task<int> GetMaxSkillIdAsync(CancellationToken cancellationToken = default);
	}
}
