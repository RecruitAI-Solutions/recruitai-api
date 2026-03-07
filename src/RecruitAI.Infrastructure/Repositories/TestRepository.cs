using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
    internal class TestRepository : ITestRepository
    {
        private readonly RecruitDevContext _context;
        private readonly ILogger<TestRepository> _logger;

        public TestRepository(RecruitDevContext context, ILogger<TestRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Test> AddAsync(Test entity, CancellationToken cancellationToken)
        {
            try
            {
                _context.Tests.Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Test entity");
                throw;
            }
        }
        public async Task<IEnumerable<Test>> GetAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                var listTest = await _context.Tests.ToListAsync();
                return listTest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Test entities");
                throw;
            }
        }
    }
}
