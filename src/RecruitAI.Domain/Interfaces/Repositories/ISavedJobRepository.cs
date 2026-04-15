using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface ISavedJobRepository : IBaseRepository<SavedJob>
{
	Task<SavedJob?> GetByJobAndUserAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default);
	Task<bool> IsSavedAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default);
	Task<(List<SavedJob> Items, int Total)> GetSavedJobsByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
	Task DeleteByJobAndUserAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default);
}