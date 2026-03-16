using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces;

public interface ICVRepository
{
	Task<CV?> GetByIdAsync(Guid id);
	Task<IEnumerable<CV>> GetByUserIdAsync(Guid userId);
	Task AddAsync(CV cv);
	Task UpdateAsync(CV cv);
	Task DeleteAsync(Guid id);
}