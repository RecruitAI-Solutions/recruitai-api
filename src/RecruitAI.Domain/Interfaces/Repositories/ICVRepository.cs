using RecruitAI.Domain.Common.CVs;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface ICVRepository
{
	Task<CV?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<IEnumerable<CV>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<(IEnumerable<CV> Items, int Total)> GetUserCVsAsync(
		Guid userId,
		CVFilter filter,
		CancellationToken cancellationToken = default);
	Task AddAsync(CV cv, CancellationToken cancellationToken = default);
	Task UpdateAsync(CV cv, CancellationToken cancellationToken = default);
	Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}