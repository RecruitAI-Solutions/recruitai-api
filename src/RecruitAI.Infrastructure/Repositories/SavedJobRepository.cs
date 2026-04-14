using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories;

public class SavedJobRepository : BaseRepository<SavedJob>, ISavedJobRepository
{
	public SavedJobRepository(RecruitDevContext context) : base(context)
	{
	}

	public async Task<SavedJob?> GetByJobAndUserAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.FirstOrDefaultAsync(s => s.JobId == jobId && s.UserId == userId, cancellationToken);
	}

	public async Task<bool> IsSavedAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default)
	{
		return await _dbSet.AnyAsync(s => s.JobId == jobId && s.UserId == userId, cancellationToken);
	}

	public async Task<(List<SavedJob> Items, int Total)> GetSavedJobsByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
	{
		var query = _dbSet
			.Include(s => s.Job)
				.ThenInclude(j => j.Company)
			.Include(s => s.Job)
				.ThenInclude(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
			.Where(s => s.UserId == userId)
			.OrderByDescending(s => s.SavedAt);

		var total = await query.CountAsync(cancellationToken);

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return (items, total);
	}

	public async Task DeleteByJobAndUserAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default)
	{
		var savedJob = await GetByJobAndUserAsync(jobId, userId, cancellationToken);
		if (savedJob != null)
		{
			_dbSet.Remove(savedJob);
		}
	}
}