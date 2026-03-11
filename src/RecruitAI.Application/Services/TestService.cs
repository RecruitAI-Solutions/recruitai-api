using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Domain.Interfaces.Services;

namespace RecruitAI.Application.Services
{
	public class TestService : ITestService
	{
		private readonly IUnitOfWork _uow; 
		private readonly ITestDomainService _testDomainService;
		private readonly ILogger<TestService> _logger;

		public TestService(
			IUnitOfWork uow, 
			ITestDomainService testDomainService,
			ILogger<TestService> logger)
		{
			_uow = uow;
			_testDomainService = testDomainService;
			_logger = logger;
		}

		public async Task<IEnumerable<TestResponseDto>> GetAllTestsAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				// Kiểm tra cancellation
				if (cancellationToken.IsCancellationRequested)
					cancellationToken.ThrowIfCancellationRequested();

				var tests = await _uow.Tests.GetAllAsync(cancellationToken);

				return tests.Select(t => new TestResponseDto
				{
					Id = t.Id,
					FirstName = t.FirstName,
					LastName = t.LastName,
					FullName = $"{t.FirstName} {t.LastName}"
				});
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("GetAllTestsAsync was cancelled");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving tests.");
				throw new TestValidationException("An error occurred while retrieving tests.", ex);
			}
		}

		public async Task<TestResponseDto> AddTestAsync(CreatedTestRequestDto test, CancellationToken cancellationToken = default)
		{
			try
			{
				// Kiểm tra cancellation
				if (cancellationToken.IsCancellationRequested)
					cancellationToken.ThrowIfCancellationRequested();

				// Validation
				if (string.IsNullOrWhiteSpace(test.FirstName))
					throw new TestValidationException("First name is required");

				if (string.IsNullOrWhiteSpace(test.LastName))
					throw new TestValidationException("Last name is required");

				// Tạo entity
				var testEntity = new Test
				{
					FirstName = test.FirstName.Trim(),
					LastName = test.LastName.Trim()
				};

				// Gọi domain service để xử lý nghiệp vụ
				await _testDomainService.ValidateAndProcessAsync(testEntity, cancellationToken);

				// Bắt đầu transaction
				await _uow.BeginTransactionAsync(cancellationToken);

				// Thêm vào database qua repository
				await _uow.Tests.AddAsync(testEntity, cancellationToken);

				// Lưu thay đổi
				await _uow.CommitTransactionAsync(cancellationToken);

				_logger.LogInformation("Test added successfully: {FirstName} {LastName}",
					testEntity.FirstName, testEntity.LastName);

				return new TestResponseDto
				{
					Id = testEntity.Id,
					FirstName = testEntity.FirstName,
					LastName = testEntity.LastName,
					FullName = $"{testEntity.FirstName} {testEntity.LastName}"
				};
			}
			catch (OperationCanceledException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogWarning("AddTestAsync was cancelled");
				throw;
			}
			catch (TestValidationException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogWarning("Validation failed for test: {FirstName} {LastName}",
					test?.FirstName, test?.LastName);
				throw;
			}
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, "Error adding test: {FirstName} {LastName}",
					test?.FirstName, test?.LastName);
				throw new TestValidationException("An error occurred while adding the test.", ex);
			}
		}

		public async Task<TestResponseDto> GetTestByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
					cancellationToken.ThrowIfCancellationRequested();

				var test = await _uow.Tests.GetByIdAsync(id, cancellationToken);

				if (test == null)
					throw new TestValidationException($"Test with id {id} not found");

				return new TestResponseDto
				{
					Id = test.Id,
					FirstName = test.FirstName,
					LastName = test.LastName,
					FullName = $"{test.FirstName} {test.LastName}"
				};
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("GetTestByIdAsync was cancelled for id {Id}", id);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting test with id {Id}", id);
				throw;
			}
		}

		public async Task<bool> DeleteTestAsync(int id, CancellationToken cancellationToken = default)
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
					cancellationToken.ThrowIfCancellationRequested();

				var test = await _uow.Tests.GetByIdAsync(id, cancellationToken);

				if (test == null)
					return false;

				await _uow.BeginTransactionAsync(cancellationToken);

				_uow.Tests.Remove(test);
				await _uow.CommitTransactionAsync(cancellationToken);

				_logger.LogInformation("Test deleted successfully: Id {Id}", id);

				return true;
			}
			catch (OperationCanceledException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogWarning("DeleteTestAsync was cancelled for id {Id}", id);
				throw;
			}
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, "Error deleting test with id {Id}", id);
				throw;
			}
		}

		public async Task<IEnumerable<TestResponseDto>> SearchTestsByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
					cancellationToken.ThrowIfCancellationRequested();

				if (string.IsNullOrWhiteSpace(name))
					return Enumerable.Empty<TestResponseDto>();

				var tests = await _uow.Tests.GetByNameAsync(name, cancellationToken);

				return tests.Select(t => new TestResponseDto
				{
					Id = t.Id,
					FirstName = t.FirstName,
					LastName = t.LastName,
					FullName = $"{t.FirstName} {t.LastName}"
				});
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("SearchTestsByNameAsync was cancelled for name {Name}", name);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error searching tests by name {Name}", name);
				throw;
			}
		}
	}
}