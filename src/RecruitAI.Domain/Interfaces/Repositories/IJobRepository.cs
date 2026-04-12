using RecruitAI.Domain.Common.Jobs;
using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface IJobRepository : IBaseRepository<Job>
{
	new Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<PagedResult<Job>> GetJobsAsync(JobFilter filter, CancellationToken cancellationToken = default);
	new Task AddAsync(Job job, CancellationToken cancellationToken = default);
	new Task UpdateAsync(Job job, CancellationToken cancellationToken = default);
	Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
	Task<bool> IsOwnerAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default);
	Task<(List<Job> Items, int Total)> GetJobsByRecruiterAsync(
		Guid recruiterId,
		JobFilter filter,
		CancellationToken cancellationToken = default);
	Task<PagedResult<Job>> GetDeletedJobsAsync(
	JobFilter filter,
	CancellationToken cancellationToken = default);
	Task AddJobSkillsAsync(Guid jobId, List<int> skillIds, bool isRequired = true);
	Task UpdateJobSkillsAsync(Guid jobId, List<int> skillIds);
	Task RemoveJobSkillsAsync(Guid jobId);
	Task<Dictionary<JobStatus, int>> CountJobsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
	Task<int[]> CountJobsByDayAsync(int days, DateTime? endDate = null, CancellationToken cancellationToken = default);
}