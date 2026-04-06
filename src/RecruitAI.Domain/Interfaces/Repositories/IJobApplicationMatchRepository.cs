using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface IJobApplicationMatchRepository : IBaseRepository<JobApplicationMatch>
{
	Task<JobApplicationMatch?> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
	Task<List<JobApplicationMatch>> GetByCvIdAsync(Guid cvId, CancellationToken cancellationToken = default);
}