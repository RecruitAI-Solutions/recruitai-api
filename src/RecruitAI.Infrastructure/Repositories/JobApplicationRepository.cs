using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;
using RecruitAI.Domain.Common.Reports;

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
	public async Task<JobApplication?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(a => a.Job)
			.Include(a => a.CV)
				.ThenInclude(cv => cv.User)
			.Include(a => a.Match)
			.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
	}
	public async Task<Dictionary<JobApplicationStatus, int>> CountApplicationsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
	{
		var query = _dbSet.AsQueryable();

		if (fromDate.HasValue)
			query = query.Where(a => a.AppliedAt >= fromDate.Value);

		if (toDate.HasValue)
		{
			var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
			query = query.Where(a => a.AppliedAt <= toDateEnd);
		}

		var items = await query
			.GroupBy(a => a.Status)
			.Select(g => new { Status = g.Key, Count = g.Count() })
			.ToListAsync(cancellationToken);

		return items.ToDictionary(x => x.Status, x => x.Count);
	}

	public async Task<int[]> CountApplicationsByDayAsync(int days, DateTime? endDate = null, CancellationToken cancellationToken = default)
	{
		var end = endDate ?? DateTime.UtcNow;
		var startDate = end.AddDays(-days + 1).Date;
		var result = new int[days];

		var items = await _dbSet
			.Where(a => a.AppliedAt >= startDate)
			.GroupBy(a => a.AppliedAt.Date)
			.Select(g => new { Date = g.Key, Count = g.Count() })
			.ToListAsync(cancellationToken);

		var dict = items.ToDictionary(x => x.Date, x => x.Count);

		for (int i = 0; i < days; i++)
		{
			var date = startDate.AddDays(i);
			result[i] = dict.ContainsKey(date) ? dict[date] : 0;
		}

		return result;
	}

	public async Task<int> CountByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
	{
		return await _context.JobApplications
			.Where(a => a.JobId == jobId)
			.CountAsync(cancellationToken);
	}
	public IQueryable<JobApplication> GetQueryable()
	{
		return _dbSet.AsQueryable();
	}
	public async Task<List<MonthlyApplicationStat>> GetApplicationsByMonthAsync(int year, JobApplicationStatus? status = null, CancellationToken cancellationToken = default)
	{
		var startDate = new DateTime(year, 1, 1);
		var endDate = new DateTime(year, 12, 31, 23, 59, 59);

		var query = _context.JobApplications
			.Where(a => a.AppliedAt >= startDate && a.AppliedAt <= endDate);

		if (status.HasValue)
			query = query.Where(a => a.Status == status.Value);

		return await query
			.GroupBy(a => a.AppliedAt.Month)
			.Select(g => new MonthlyApplicationStat
			{
				Month = g.Key,
				Total = g.Count(),
				Pending = g.Count(a => a.Status == JobApplicationStatus.Pending),
				Reviewed = g.Count(a => a.Status == JobApplicationStatus.Reviewed),
				Accepted = g.Count(a => a.Status == JobApplicationStatus.Accepted),
				Rejected = g.Count(a => a.Status == JobApplicationStatus.Rejected)
			})
			.ToListAsync(cancellationToken);
	}

}