using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Domain.Interfaces.Services;

namespace RecruitAI.Application.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;

        public TestService(ITestRepository testRepository)
        {
            _testRepository = testRepository;
        }

        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            return await _testRepository.GetAllAsync();
        }

        public async Task<Test> AddTestAsync(Test test)
        {
            return await _testRepository.AddAsync(test);
        }
    }
}
