using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Services
{
    public interface ITestService
    {
        Task<IEnumerable<Test>> GetAllTestsAsync();
        Task<Test> AddTestAsync(Test test);
    }
}
