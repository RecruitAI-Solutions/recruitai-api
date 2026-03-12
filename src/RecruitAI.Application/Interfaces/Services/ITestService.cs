using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;

namespace RecruitAI.Application.Interfaces.Services
{
    public interface ITestService
    {
        Task<IEnumerable<TestResponseDto>> GetAllTestsAsync(CancellationToken cancellationToken = default);
        Task<TestResponseDto> AddTestAsync(CreatedTestRequestDto test, CancellationToken cancellationToken = default);
        Task<TestResponseDto> GetTestByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> DeleteTestAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TestResponseDto>> SearchTestsByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}