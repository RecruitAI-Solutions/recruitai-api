using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Services
{
    public interface ITestDomainService
    {
        Task ValidateAndProcessAsync(Test test, CancellationToken cancellationToken);
        bool IsValid(Test test, CancellationToken cancellationToken);

    }
}
