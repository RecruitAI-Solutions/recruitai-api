using RecruitAI.Application.Interfaces.Repositories;
using RecruitAI.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
		ICVRepository CVs{ get; }
		ISkillRepository Skills { get; }

		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
		Task BeginTransactionAsync(CancellationToken cancellationToken = default);
		Task CommitTransactionAsync(CancellationToken cancellationToken = default);
		Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
		bool HasActiveTransaction { get; }
	}
}
