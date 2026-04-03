using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Repositories;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Repositories;

namespace RecruitAI.Infrastructure.Data
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly RecruitDevContext _context;
		private IDbContextTransaction _currentTransaction;
		private readonly ILogger<UnitOfWork> _logger;
		private readonly ILogger<TestRepository> _testLogger;

		// Repositories
		private ITestRepository _testRepository;
		private IUserRepository _userRepository;
		private IAuthProviderRepository _authProviderRepository;
		private IRefreshTokenRepository _refreshTokenRepository;
		private IPasswordResetTokenRepository _passwordResetTokenRepository;
		private IJobRepository _jobRepository;
		private ICVRepository _cvRepository;
		private ISkillRepository _skillRepository;
		private ICVAnalysisRepository? _cvAnalysisRepository;

		public UnitOfWork(RecruitDevContext context,
			ILogger<UnitOfWork> logger)
		{
			_context = context;
			_logger = logger;
		}

		// Properties - khởi tạo repository khi cần
		public ITestRepository Tests =>
			_testRepository ??= new TestRepository(_context, _testLogger);
		public IUserRepository Users =>
			_userRepository ??= new UserRepository(_context);

		public IAuthProviderRepository AuthProviders =>
			_authProviderRepository ??= new AuthProviderRepository(_context);

		public IRefreshTokenRepository RefreshTokens =>
			_refreshTokenRepository ??= new RefreshTokenRepository(_context);
		public IPasswordResetTokenRepository PasswordResetTokens =>
			_passwordResetTokenRepository ??= new PasswordResetTokenRepository(_context);
		public IJobRepository Jobs =>
			_jobRepository ??= new JobRepository(_context);
		public ICVRepository CVs =>
			_cvRepository ??= new CVRepository(_context);
		public ISkillRepository Skills =>
			_skillRepository ??= new SkillRepository(_context);
		public ICVAnalysisRepository CVAnalysisResults =>
	_cvAnalysisRepository ??= new CVAnalysisRepository(_context);



		public bool HasActiveTransaction => _currentTransaction != null;

		/// <summary>
		/// Lưu tất cả thay đổi vào database
		/// </summary>
		public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return await _context.SaveChangesAsync(cancellationToken);
		}

		/// <summary>
		/// Bắt đầu transaction
		/// </summary>
		public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
		{
			if (_currentTransaction != null)
				return;

			_currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		}

		/// <summary>
		/// Commit transaction
		/// </summary>
		public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				await SaveChangesAsync(cancellationToken);

				if (_currentTransaction != null)
				{
					await _currentTransaction.CommitAsync(cancellationToken);
				}
			}
			catch
			{
				await RollbackTransactionAsync(cancellationToken);
				throw;
			}
			finally
			{
				if (_currentTransaction != null)
				{
					await _currentTransaction.DisposeAsync();
					_currentTransaction = null;
				}
			}
		}

		/// <summary>
		/// Rollback transaction
		/// </summary>
		public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				if (_currentTransaction != null)
				{
					await _currentTransaction.RollbackAsync(cancellationToken);
				}
			}
			finally
			{
				if (_currentTransaction != null)
				{
					await _currentTransaction.DisposeAsync();
					_currentTransaction = null;
				}
			}
		}

		/// <summary>
		/// Giải phóng tài nguyên
		/// </summary>
		public void Dispose()
		{
			_currentTransaction?.Dispose();
			_context.Dispose();
		}
	}
}