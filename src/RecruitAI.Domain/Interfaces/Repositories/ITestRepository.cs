using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories
{
    public interface ITestRepository
    {
        Task<IEnumerable<Test>> GetAllAsync();
        Task<Test> AddAsync(Test entity);
    }
}
