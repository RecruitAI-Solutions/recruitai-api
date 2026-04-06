using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
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
			.Include(x => x.CV)
			.Include(x => x.Match)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<JobApplication>> GetByCvIdAsync(Guid cvId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(x => x.CVId == cvId)
			.Include(x => x.Job)
			.Include(x => x.Match)
			.ToListAsync(cancellationToken);
	}

	public async Task<PagedResult<JobApplication>> GetByUserIdAsync(Guid userId, int page, int pageSize, JobApplicationStatus? status, CancellationToken cancellationToken = default)
	{
		var query = _dbSet
			.Include(x => x.Job)
			.Include(x => x.Match)
			.Where(x => x.CV.UserId == userId);

		if (status.HasValue)
		{
			query = query.Where(x => x.Status == status.Value);
		}

		var total = await query.CountAsync(cancellationToken);

		var items = await query
			.OrderByDescending(x => x.AppliedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return new PagedResult<JobApplication>
		{
			Items = items,
			Total = total
		};
	}

	public async Task<PagedResult<JobApplication>> GetByJobIdWithFilterAsync(
		Guid jobId, int page, int pageSize, JobApplicationStatus? status, int? minMatch,
		string sortBy, string sortOrder, CancellationToken cancellationToken = default)
	{
		var query = _dbSet
			.Include(x => x.CV)
			.Include(x => x.CV.User)
			.Include(x => x.Match)
			.Where(x => x.JobId == jobId);

		if (status.HasValue)
		{
			query = query.Where(x => x.Status == status.Value);
		}

		if (minMatch.HasValue && minMatch.Value > 0)
		{
			query = query.Where(x => x.Match != null && x.Match.MatchPercentage >= minMatch.Value);
		}

		var total = await query.CountAsync(cancellationToken);

		// Apply sorting
		query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
		{
			("matchpercentage", "asc") => query.OrderBy(x => x.Match!.MatchPercentage),
			("matchpercentage", "desc") => query.OrderByDescending(x => x.Match!.MatchPercentage),
			("appliedat", "asc") => query.OrderBy(x => x.AppliedAt),
			_ => query.OrderByDescending(x => x.AppliedAt)
		};

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return new PagedResult<JobApplication>
		{
			Items = items,
			Total = total
		};
	}

	public async Task<bool> HasAppliedAsync(Guid jobId, Guid cvId, CancellationToken cancellationToken = default)
	{
		return await _dbSet.AnyAsync(x => x.JobId == jobId && x.CVId == cvId, cancellationToken);
	}

	public async Task<int> GetApplicationCountByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
	{
		return await _dbSet.CountAsync(x => x.JobId == jobId, cancellationToken);
	}
}