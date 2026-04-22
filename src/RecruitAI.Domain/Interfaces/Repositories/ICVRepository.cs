using RecruitAI.Domain.Common.CVs;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface ICVRepository : IBaseRepository<CV>
{
	new Task<CV?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<IEnumerable<CV>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<(IEnumerable<CVList> Items, int Total)> GetUserCVsAsync(
		Guid userId,
		CVFilter filter,
		CancellationToken cancellationToken = default);
	new Task AddAsync(CV cv, CancellationToken cancellationToken = default);
	new Task UpdateAsync(CV cv, CancellationToken cancellationToken = default);
	Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
	Task<Dictionary<CVStatus, int>> CountCVsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
	Task<int[]> CountCVsByDayAsync(int days, DateTime? endDate = null, CancellationToken cancellationToken = default);
}