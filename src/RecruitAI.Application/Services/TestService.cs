using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Domain.Interfaces.Services;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Application.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;
        private readonly ITestDomainService _testDomainService;
        private readonly ILogger<TestService> _logger;
        private readonly RecruitDevContext _context;

        public TestService(RecruitDevContext context, ITestRepository testRepository, ITestDomainService testDomainService, ILogger<TestService> logger)
        {
            _testRepository = testRepository;
            _testDomainService = testDomainService;
            _logger = logger;
            _context = context;
        }

        public async Task<IEnumerable<CreatedTestResponseDto>> GetAllTestsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var tests = await _testRepository.GetAllAsync(cancellationToken);
                return tests.Select(t =>
                {
                    return new CreatedTestResponseDto
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName
                    };
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tests.");
                throw new TestValidationException("An error occurred while retrieving tests.", ex);
            }
        }

        public async Task<CreatedTestResponseDto> AddTestAsync(CreatedTestRequestDto test, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(test.FirstName))
                    throw new TestValidationException(nameof(test.FirstName));

                if (string.IsNullOrWhiteSpace(test.LastName))
                    throw new TestValidationException(nameof(test.LastName));

                var testEntity = new Test
                {
                    FirstName = test.FirstName,
                    LastName = test.LastName
                };

                await _testDomainService.ValidateAndProcessAsync(testEntity, cancellationToken);

                var created = await _testRepository.AddAsync(testEntity, cancellationToken);

                return new CreatedTestResponseDto
                {
                    Id = created.Id,
                    FirstName = created.FirstName,
                    LastName = created.LastName
                };
            }
            catch (TestValidationException)
            {
                _logger.LogWarning("Validation failed for adding a test: {FirstName} {LastName}", test.FirstName, test.LastName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a test: {FirstName} {LastName}", test.FirstName, test.LastName);
                throw new TestValidationException("An error occurred while adding the test.", ex);
            }
        }
    }
}
