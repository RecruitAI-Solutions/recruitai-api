using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories
{
	public interface ICVAnalysisRepository: IBaseRepository<CVAnalysisResult>
	{
		Task<CVAnalysisResult?> GetByIdAsync(Guid id);
		Task<IEnumerable<CVAnalysisResult>> GetByCVIdAsync(Guid cvId);
		Task<CVAnalysisResult?> GetByCVAndSkillAsync(Guid cvId, int skillId);
		Task AddAsync(CVAnalysisResult result);
		Task AddRangeAsync(IEnumerable<CVAnalysisResult> results);
		Task RemoveByCVIdAsync(Guid cvId);
		Task<bool> HasAnalysisAsync(Guid cvId);
		Task<List<CVAnalysisResult>> GetByCvIdAsync(Guid cvId);
		Task<CVAnalysisResult?> GetByCvIdAndSkillIdAsync(Guid cvId, int skillId);
		Task DeleteByCvIdAsync(Guid cvId);
		IQueryable<CVAnalysisResult> GetQueryable();
	}
}