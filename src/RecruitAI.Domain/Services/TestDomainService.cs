using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Exceptions; // Thêm using
using RecruitAI.Domain.Interfaces.Services;

namespace RecruitAI.Domain.Services
{
    public class TestDomainService : ITestDomainService
    {
        public async Task ValidateAndProcessAsync(Test test, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(test.FirstName))
                    throw new DomainException("First name is required");

                if (string.IsNullOrWhiteSpace(test.LastName))
                    throw new DomainException("Last name is required");

                await Task.CompletedTask;
            }
            catch (DomainException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DomainException("An unexpected error occurred", ex);
            }
        }
        public bool IsValid(Test test, CancellationToken cancellationToken)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(test.FirstName)
                    && !string.IsNullOrWhiteSpace(test.LastName);
            }
            catch (Exception ex)
            {
                throw new DomainException("An unexpected error occurred", ex);
            }
        }
    }
}