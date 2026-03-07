using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;

namespace RecruitAI.Application.Interfaces.Services
{
    public interface ITestService
    {
        Task<IEnumerable<CreatedTestResponseDto>> GetAllTestsAsync(CancellationToken cancellationToken);
        Task<CreatedTestResponseDto> AddTestAsync(CreatedTestRequestDto test, CancellationToken cancellationToken);
    }
}
