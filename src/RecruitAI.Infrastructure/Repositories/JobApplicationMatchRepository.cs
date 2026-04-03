using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories;

public class JobApplicationMatchRepository : BaseRepository<JobApplicationMatch>, IJobApplicationMatchRepository
{
	public JobApplicationMatchRepository(RecruitDevContext context) : base(context)
	{
	}

	public async Task<JobApplicationMatch?> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.FirstOrDefaultAsync(x => x.ApplicationId == applicationId, cancellationToken);
	}

	public async Task<List<JobApplicationMatch>> GetByCvIdAsync(Guid cvId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(x => x.Application)
			.Where(x => x.Application.CVId == cvId)
			.ToListAsync(cancellationToken);
	}
	public async Task UpdateAsync(JobApplicationMatch match)
	{
		_dbSet.Update(match);
		await Task.CompletedTask;
	}
}