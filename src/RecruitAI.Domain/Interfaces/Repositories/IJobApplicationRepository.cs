using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface IJobApplicationRepository : IBaseRepository<JobApplication> 
{
	Task<JobApplication?> GetByJobAndCvAsync(Guid jobId, Guid cvId, CancellationToken cancellationToken = default);
	Task<List<JobApplication>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
	Task<List<JobApplication>> GetByCvIdAsync(Guid cvId, CancellationToken cancellationToken = default);
	Task<PagedResult<JobApplication>> GetByUserIdAsync(Guid userId, int page, int pageSize, JobApplicationStatus? status, CancellationToken cancellationToken = default);
	Task<PagedResult<JobApplication>> GetByJobIdWithFilterAsync(Guid jobId, int page, int pageSize, JobApplicationStatus? status, int? minMatch, string sortBy, string sortOrder, CancellationToken cancellationToken = default);
	Task<bool> HasAppliedAsync(Guid jobId, Guid cvId, CancellationToken cancellationToken = default);
	Task<int> GetApplicationCountByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
	Task<JobApplication?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<Dictionary<JobApplicationStatus, int>> CountApplicationsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
	Task<int[]> CountApplicationsByDayAsync(int days, DateTime? endDate = null, CancellationToken cancellationToken = default);
	Task<int> CountByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
}