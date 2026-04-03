using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories;

public class JobApplicationRepository : BaseRepository<JobApplication>, IJobApplicationRepository
{
	public JobApplicationRepository(RecruitDevContext context) : base(context)
	{
	}

	public async Task<JobApplication?> GetByJobAndCvAsync(Guid jobId, Guid cvId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.FirstOrDefaultAsync(x => x.JobId == jobId && x.CVId == cvId, cancellationToken);
	}

	public async Task<List<JobApplication>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(x => x.JobId == jobId)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<JobApplication>> GetByCvIdAsync(Guid cvId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(x => x.CVId == cvId)
			.ToListAsync(cancellationToken);
	}
}