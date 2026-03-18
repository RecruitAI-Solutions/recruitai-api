using RecruitAI.Domain.Common;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface ICVRepository
{
	Task<CV?> GetByIdAsync(Guid id);
	Task<IEnumerable<CV>> GetByUserIdAsync(Guid userId);
	Task<(IEnumerable<CV> Items, int Total)> GetUserCVsAsync(
		Guid userId,
		CVFilter filter,
		CancellationToken cancellationToken = default);
	Task AddAsync(CV cv);
	Task UpdateAsync(CV cv);
	Task DeleteAsync(Guid id);
}