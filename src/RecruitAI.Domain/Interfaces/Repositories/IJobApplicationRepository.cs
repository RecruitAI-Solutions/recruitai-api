using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface IJobApplicationRepository : IBaseRepository<JobApplication>
{
	Task<JobApplication?> GetByJobAndCvAsync(Guid jobId, Guid cvId, CancellationToken cancellationToken = default);
	Task<List<JobApplication>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
	Task<List<JobApplication>> GetByCvIdAsync(Guid cvId, CancellationToken cancellationToken = default);
}