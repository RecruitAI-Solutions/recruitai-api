using RecruitAI.Domain.Common;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface IJobRepository
{
	Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<PagedResult<Job>> GetJobsAsync(JobFilter filter, CancellationToken cancellationToken = default);
	Task AddAsync(Job job, CancellationToken cancellationToken = default);
	Task UpdateAsync(Job job, CancellationToken cancellationToken = default);
	Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
	Task<bool> IsOwnerAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default);
	Task<(List<Job> Items, int Total)> GetJobsByRecruiterAsync(
		Guid recruiterId,
		JobFilter filter,
		CancellationToken cancellationToken = default);
}