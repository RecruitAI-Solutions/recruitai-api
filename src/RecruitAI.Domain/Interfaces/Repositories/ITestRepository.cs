using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories
{
    public interface ITestRepository
    {
        Task<IEnumerable<Test>> GetAllAsync(CancellationToken cancellationToken);
        Task<Test> AddAsync(Test entity, CancellationToken cancellationToken);
    }
}
