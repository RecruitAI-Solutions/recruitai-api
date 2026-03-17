using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Domain.Entities;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
	public class TestRepository : BaseRepository<Test>, ITestRepository
	{
		private readonly ILogger<TestRepository> _logger;

		public TestRepository(RecruitDevContext context, ILogger<TestRepository> logger)
			: base(context)
		{
			_logger = logger;
		}

		public async Task<IEnumerable<Test>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			try
			{
				// Dùng _dbSet từ BaseRepository
				return await _dbSet
					.Where(t => t.FirstName.Contains(name) || t.LastName.Contains(name))
					.ToListAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error searching tests by name: {Name}", name);
				throw;
			}
		}

		public override async Task<Test> AddAsync(Test entity, CancellationToken cancellationToken = default)
		{
			// Thêm logic trước khi add nếu cần
			_logger.LogDebug("Adding test: {FirstName} {LastName}", entity.FirstName, entity.LastName);

			// Gọi base
			await base.AddAsync(entity, cancellationToken);
			await _context.SaveChangesAsync(cancellationToken);

			return entity;
		}

		public async Task<Test> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
		}
	}
}