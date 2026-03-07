using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
    internal class TestRepository : ITestRepository
    {
        private readonly RecruitDevContext _context;

        public TestRepository(RecruitDevContext context)
        {
            _context = context;
        }

        public async Task<Test> AddAsync(Test entity)
        {
            _context.Tests.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<IEnumerable<Test>> GetAllAsync()
        {
            var listTest = await _context.Tests.ToListAsync();
            return listTest;
        }
    }
}
