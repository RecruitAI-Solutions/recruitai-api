// RecruitAI.Infrastructure/Repositories/CVAnalysisRepository.cs
using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
	public class CVAnalysisRepository : BaseRepository<CVAnalysisResult>, ICVAnalysisRepository
	{
		public CVAnalysisRepository(RecruitDevContext context) : base(context)
		{
		}

		public async Task<CVAnalysisResult?> GetByIdAsync(Guid id)
		{
			return await _dbSet.FindAsync(id);
		}

		public async Task<IEnumerable<CVAnalysisResult>> GetByCVIdAsync(Guid cvId)
		{
			return await _dbSet
				.Include(r => r.Skill)
				.Where(r => r.CVId == cvId)
				.OrderByDescending(r => r.Confidence)
				.ToListAsync();
		}

		public async Task<CVAnalysisResult?> GetByCVAndSkillAsync(Guid cvId, int skillId)
		{
			return await _dbSet
				.FirstOrDefaultAsync(r => r.CVId == cvId && r.SkillId == skillId);
		}

		public async Task AddAsync(CVAnalysisResult result)
		{
			await _dbSet.AddAsync(result);
		}

		public async Task AddRangeAsync(IEnumerable<CVAnalysisResult> results)
		{
			await _dbSet.AddRangeAsync(results);
		}

		public async Task RemoveByCVIdAsync(Guid cvId)
		{
			var results = await _dbSet.Where(r => r.CVId == cvId).ToListAsync();
			_dbSet.RemoveRange(results);
		}

		public async Task<bool> HasAnalysisAsync(Guid cvId)
		{
			return await _dbSet.AnyAsync(r => r.CVId == cvId);
		}

		public async Task<List<CVAnalysisResult>> GetByCvIdAsync(Guid cvId)
		{
			return await _dbSet
				.Include(r => r.Skill)
				.Where(r => r.CVId == cvId)
				.ToListAsync();
		}

		public async Task<CVAnalysisResult?> GetByCvIdAndSkillIdAsync(Guid cvId, int skillId)
		{
			return await _dbSet
				.FirstOrDefaultAsync(r => r.CVId == cvId && r.SkillId == skillId);
		}

		public async Task DeleteByCvIdAsync(Guid cvId)
		{
			var results = await _dbSet
				.Where(r => r.CVId == cvId)
				.ToListAsync();

			if (results.Any())
			{
				_dbSet.RemoveRange(results);
			}
		}
		public IQueryable<CVAnalysisResult> GetQueryable()
		{
			return _dbSet.AsQueryable();
		}
	}
}