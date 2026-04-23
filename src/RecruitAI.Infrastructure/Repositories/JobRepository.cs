using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Common.Jobs;
using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Common.Reports;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories;

public class JobRepository : BaseRepository<Job>, IJobRepository
{
	private readonly RecruitDevContext _context;

	public JobRepository(RecruitDevContext context) : base(context)
	{
		_context = context;
	}

	public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(j => j.Recruiter)
			.Include(j => j.Company)
			.Include(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
			.FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted, cancellationToken);
	}

	private IQueryable<Job> ApplySkillFilter(IQueryable<Job> query, JobFilter filter)
	{
		if (!string.IsNullOrWhiteSpace(filter.Skill))
		{
			var search = filter.Skill.Trim();
			query = query.Where(j => j.JobSkills.Any(js =>
				js.Skill.Name.Contains(search) ||
				(js.Skill.Aliases != null && js.Skill.Aliases.Contains(search))));
		}

		if (filter.Skills != null && filter.Skills.Any())
		{
			var skills = filter.Skills
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s.Trim())
				.Distinct()
				.ToList();

			if (skills.Any())
			{
				if (filter.MatchAllSkills)
				{
					var jobIds = _context.JobSkills
						.Where(js => skills.Any(sk => js.Skill.Name.Contains(sk) || (js.Skill.Aliases != null && js.Skill.Aliases.Contains(sk))))
						.GroupBy(js => js.JobId)
						.Where(g => g.Select(js => js.SkillId).Distinct().Count() == skills.Count)
						.Select(g => g.Key);

					query = query.Where(j => jobIds.Contains(j.Id));
				}
				else
				{
					query = query.Where(j => j.JobSkills.Any(js =>
						skills.Any(sk => js.Skill.Name.Contains(sk) || (js.Skill.Aliases != null && js.Skill.Aliases.Contains(sk)))));
				}
			}
		}

		return query;
	}

	public async Task<PagedResult<Job>> GetJobsAsync(
	JobFilter filter,
	CancellationToken cancellationToken = default)
	{
		var query = _dbSet
			.Include(j => j.Recruiter)
			.Include(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
			.Where(j => !j.IsDeleted);

		// Apply filters
		if (!string.IsNullOrWhiteSpace(filter.Title))
		{
			query = query.Where(j => j.Title.Contains(filter.Title));
		}

		if (!string.IsNullOrWhiteSpace(filter.Location))
		{
			query = query.Where(j => j.Location.Contains(filter.Location));
		}

		if (filter.MinSalary.HasValue)
		{
			query = query.Where(j => j.SalaryMax >= filter.MinSalary.Value);
		}

		if (filter.MaxSalary.HasValue)
		{
			query = query.Where(j => j.SalaryMin <= filter.MaxSalary.Value);
		}

		// Filter EmploymentType (hỗ trợ nhiều giá trị)
		if (filter.EmploymentType != null && filter.EmploymentType.Any())
		{
			query = query.Where(j => j.EmploymentType.HasValue && filter.EmploymentType.Contains(j.EmploymentType.Value));
		}

		// Filter ExperienceLevel (hỗ trợ nhiều giá trị)
		if (filter.ExperienceLevel != null && filter.ExperienceLevel.Any())
		{
			query = query.Where(j => j.ExperienceLevel.HasValue && filter.ExperienceLevel.Contains(j.ExperienceLevel.Value));
		}

		query = ApplySkillFilter(query, filter);

		// Get total count before pagination
		var total = await query.CountAsync(cancellationToken);

		// Apply sorting
		query = filter.SortBy?.ToLower() switch
		{
			"title" => filter.SortOrder == "asc"
				? query.OrderBy(j => j.Title)
				: query.OrderByDescending(j => j.Title),
			"salary" => filter.SortOrder == "asc"
				? query.OrderBy(j => j.SalaryMin)
				: query.OrderByDescending(j => j.SalaryMin),
			_ => filter.SortOrder == "asc"
				? query.OrderBy(j => j.CreatedAt)
				: query.OrderByDescending(j => j.CreatedAt)
		};

		// Apply pagination
		var items = await query
			.Skip((filter.Page - 1) * filter.PageSize)
			.Take(filter.PageSize)
			.ToListAsync(cancellationToken);

		return new PagedResult<Job>
		{
			Items = items,
			Total = total
		};
	}

	public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
	{
		await _dbSet.AddAsync(job, cancellationToken);
	}

	public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
	{
		_dbSet.Update(job);
	}

	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var job = await GetByIdAsync(id, cancellationToken);
		if (job != null)
		{
			job.IsDeleted = true;
			_dbSet.Update(job);
		}
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.AnyAsync(j => j.Id == id && !j.IsDeleted, cancellationToken);
	}

	public async Task<bool> IsOwnerAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.AnyAsync(j => j.Id == jobId && j.RecruiterId == userId && !j.IsDeleted, cancellationToken);
	}

	public async Task<(List<Job> Items, int Total)> GetJobsByRecruiterAsync(
	Guid recruiterId,
	JobFilter filter,
	CancellationToken cancellationToken = default)
	{
		var query = _dbSet
			.Include(j => j.Recruiter)
			.Include(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
			.Where(j => !j.IsDeleted && j.RecruiterId == recruiterId);

		// Apply filters
		if (!string.IsNullOrWhiteSpace(filter.Title))
			query = query.Where(j => j.Title.Contains(filter.Title));

		if (!string.IsNullOrWhiteSpace(filter.Location))
			query = query.Where(j => j.Location.Contains(filter.Location));

		if (filter.MinSalary.HasValue)
			query = query.Where(j => j.SalaryMax >= filter.MinSalary.Value);

		if (filter.MaxSalary.HasValue)
			query = query.Where(j => j.SalaryMin <= filter.MaxSalary.Value);

		// Filter EmploymentType (hỗ trợ nhiều giá trị)
		if (filter.EmploymentType != null && filter.EmploymentType.Any())
		{
			query = query.Where(j => j.EmploymentType.HasValue && filter.EmploymentType.Contains(j.EmploymentType.Value));
		}

		// Filter ExperienceLevel (hỗ trợ nhiều giá trị)
		if (filter.ExperienceLevel != null && filter.ExperienceLevel.Any())
		{
			query = query.Where(j => j.ExperienceLevel.HasValue && filter.ExperienceLevel.Contains(j.ExperienceLevel.Value));
		}

		query = ApplySkillFilter(query, filter);

		var total = await query.CountAsync(cancellationToken);

		// Sorting
		query = filter.SortBy?.ToLower() switch
		{
			"title" => filter.SortOrder == "asc"
				? query.OrderBy(j => j.Title)
				: query.OrderByDescending(j => j.Title),
			"salary" => filter.SortOrder == "asc"
				? query.OrderBy(j => j.SalaryMin)
				: query.OrderByDescending(j => j.SalaryMin),
			_ => filter.SortOrder == "asc"
				? query.OrderBy(j => j.CreatedAt)
				: query.OrderByDescending(j => j.CreatedAt)
		};

		var items = await query
			.Skip((filter.Page - 1) * filter.PageSize)
			.Take(filter.PageSize)
			.ToListAsync(cancellationToken);

		return (items, total);
	}

	public async Task<PagedResult<Job>> GetDeletedJobsAsync(
	JobFilter filter,
	CancellationToken cancellationToken = default)
	{
		var query = _dbSet
			.Include(j => j.Recruiter)
			.Include(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
			.Where(j => j.IsDeleted);

		// Apply filters
		if (!string.IsNullOrWhiteSpace(filter.Title))
			query = query.Where(j => j.Title.Contains(filter.Title));

		if (!string.IsNullOrWhiteSpace(filter.Location))
			query = query.Where(j => j.Location.Contains(filter.Location));

		if (filter.MinSalary.HasValue)
			query = query.Where(j => j.SalaryMax >= filter.MinSalary.Value);

		if (filter.MaxSalary.HasValue)
			query = query.Where(j => j.SalaryMin <= filter.MaxSalary.Value);

		// Filter EmploymentType (hỗ trợ nhiều giá trị)
		if (filter.EmploymentType != null && filter.EmploymentType.Any())
		{
			query = query.Where(j => j.EmploymentType.HasValue && filter.EmploymentType.Contains(j.EmploymentType.Value));
		}

		// Filter ExperienceLevel (hỗ trợ nhiều giá trị)
		if (filter.ExperienceLevel != null && filter.ExperienceLevel.Any())
		{
			query = query.Where(j => j.ExperienceLevel.HasValue && filter.ExperienceLevel.Contains(j.ExperienceLevel.Value));
		}

		if (!string.IsNullOrWhiteSpace(filter.Skill))
		{
			query = query.Where(j => j.JobSkills.Any(js =>
				js.Skill.Name.Contains(filter.Skill) ||
				(js.Skill.Aliases != null && js.Skill.Aliases.Contains(filter.Skill))));
		}

		// Get total count
		var total = await query.CountAsync(cancellationToken);

		// Apply sorting
		query = filter.SortBy?.ToLower() switch
		{
			"title" => filter.SortOrder == "asc"
				? query.OrderBy(j => j.Title)
				: query.OrderByDescending(j => j.Title),
			"salary" => filter.SortOrder == "asc"
				? query.OrderBy(j => j.SalaryMin)
				: query.OrderByDescending(j => j.SalaryMin),
			"expirationdate" => filter.SortOrder == "asc"
				? query.OrderBy(j => j.ExpirationDate)
				: query.OrderByDescending(j => j.ExpirationDate),
			"views" => filter.SortOrder == "asc"
				? query.OrderBy(j => j.Views)
				: query.OrderByDescending(j => j.Views),
			"applications" => filter.SortOrder == "asc"
				? query.OrderBy(j => j.Applications)
				: query.OrderByDescending(j => j.Applications),
			_ => filter.SortOrder == "asc"
				? query.OrderBy(j => j.CreatedAt)
				: query.OrderByDescending(j => j.CreatedAt)
		};

		// Apply pagination
		var items = await query
			.Skip((filter.Page - 1) * filter.PageSize)
			.Take(filter.PageSize)
			.ToListAsync(cancellationToken);

		return new PagedResult<Job>
		{
			Items = items,
			Total = total
		};
	}

	public async Task AddJobSkillsAsync(Guid jobId, List<int> skillIds, bool isRequired = true)
	{
		var jobSkills = skillIds.Select(skillId => new JobSkill
		{
			JobId = jobId,
			SkillId = skillId,
			IsRequired = isRequired
		});

		await _context.JobSkills.AddRangeAsync(jobSkills);
	}

	public async Task UpdateJobSkillsAsync(Guid jobId, List<int> skillIds)
	{
		// Xóa skills cũ
		var existingSkills = await _context.JobSkills
			.Where(js => js.JobId == jobId)
			.ToListAsync();

		_context.JobSkills.RemoveRange(existingSkills);

		// Thêm skills mới
		if (skillIds != null && skillIds.Any())
		{
			// Kiểm tra skill có tồn tại không
			var validSkillIds = await _context.Skills
				.Where(s => skillIds.Contains(s.Id))
				.Select(s => s.Id)
				.ToListAsync();

			if (validSkillIds.Count != skillIds.Count)
			{
				var invalidIds = skillIds.Except(validSkillIds);
			}

			var jobSkills = validSkillIds.Select(skillId => new JobSkill
			{
				JobId = jobId,
				SkillId = skillId,
				IsRequired = true
			});

			await _context.JobSkills.AddRangeAsync(jobSkills);
		}
	}


	public async Task RemoveJobSkillsAsync(Guid jobId)
	{
		var skills = await _context.JobSkills
			.Where(js => js.JobId == jobId)
			.ToListAsync();

		_context.JobSkills.RemoveRange(skills);
	}
	public async Task<Dictionary<JobStatus, int>> CountJobsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
	{
		var query = _dbSet.Where(j => !j.IsDeleted);

		if (fromDate.HasValue)
			query = query.Where(j => j.CreatedAt >= fromDate.Value);

		if (toDate.HasValue)
		{
			var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
			query = query.Where(j => j.CreatedAt <= toDateEnd);
		}

		var items = await query
			.GroupBy(j => j.Status)
			.Select(g => new { Status = g.Key, Count = g.Count() })
			.ToListAsync(cancellationToken);

		return items.ToDictionary(x => x.Status, x => x.Count);
	}

	public async Task<int[]> CountJobsByDayAsync(int days, DateTime? endDate = null, CancellationToken cancellationToken = default)
	{
		var end = endDate ?? DateTime.UtcNow;
		var startDate = end.AddDays(-days + 1).Date;
		var result = new int[days];

		var items = await _dbSet
			.Where(j => j.CreatedAt >= startDate && !j.IsDeleted)
			.GroupBy(j => j.CreatedAt.Date)
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
	public async Task<(List<Job> Items, int Total)> GetJobsByCompanyAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default)
	{
		var query = _dbSet
			.Include(j => j.Recruiter)
			.Include(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
			.Where(j => j.CompanyId == companyId && !j.IsDeleted && j.IsActive);

		var total = await query.CountAsync(cancellationToken);

		var items = await query
			.OrderByDescending(j => j.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return (items, total);
	}
	// Lấy job đã được đánh dấu nổi bật
	public async Task<List<Job>> GetFeaturedJobsAsync(int limit, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(j => j.Recruiter)
			.Include(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
			.Include(j => j.Company)
			.Where(j => !j.IsDeleted && j.IsActive && j.IsFeatured && j.ExpirationDate > DateTime.UtcNow)
			.OrderBy(j => j.FeaturedOrder ?? int.MaxValue)
			.ThenByDescending(j => j.CreatedAt)
			.Take(limit)
			.ToListAsync(cancellationToken);
	}

	// Gợi ý job dựa trên kỹ năng
	public async Task<List<Job>> GetSimilarJobsAsync(List<int> skillIds, Guid excludeJobId, int limit, CancellationToken cancellationToken = default)
	{
		if (skillIds == null || !skillIds.Any())
			return new List<Job>();

		return await _dbSet
			.Include(j => j.Recruiter)
			.Include(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
			.Include(j => j.Company)
			.Where(j => !j.IsDeleted && j.IsActive && j.Id != excludeJobId && j.ExpirationDate > DateTime.UtcNow)
			.Where(j => j.JobSkills.Any(js => skillIds.Contains(js.SkillId)))
			.Select(j => new { Job = j, MatchCount = j.JobSkills.Count(js => skillIds.Contains(js.SkillId)) })
			.OrderByDescending(x => x.MatchCount)
			.ThenByDescending(x => x.Job.CreatedAt)
			.Select(x => x.Job)
			.Take(limit)
			.ToListAsync(cancellationToken);
	}
	public async Task<List<Job>> GetTopJobsByScoreAsync(int limit, CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		var jobs = await _dbSet
			.Where(j => !j.IsDeleted && j.IsActive && j.ExpirationDate > now)
			.ToListAsync(cancellationToken);

		if (!jobs.Any())
			return new List<Job>();

		// Tìm max views và max applications để chuẩn hóa về thang 0-100
		var maxViews = jobs.Max(j => j.Views);
		var maxApplications = jobs.Max(j => j.Applications);

		var scored = jobs.Select(j => new
		{
			Job = j,
			// Chuẩn hóa views về 0-100 (nếu max = 0 thì điểm = 0)
			ViewScore = maxViews > 0 ? (double)j.Views / maxViews * 100 : 0,
			// Chuẩn hóa applications về 0-100
			ApplicationScore = maxApplications > 0 ? (double)j.Applications / maxApplications * 100 : 0,
			// Độ mới: 30 ngày = 0 điểm, 0 ngày = 100 điểm
			FreshnessScore = Math.Max(0, 100 - (now - j.CreatedAt).TotalDays * (100.0 / 30))
		});

		var result = scored
			.Select(x => new
			{
				x.Job,
				TotalScore = (x.ViewScore * 0.3) + (x.ApplicationScore * 0.5) + (x.FreshnessScore * 0.2)
			})
			.OrderByDescending(x => x.TotalScore)
			.Take(limit)
			.Select(x => x.Job)
			.ToList();

		return result;
	}

	public async Task ResetAllFeaturedAsync(CancellationToken cancellationToken = default)
	{
		await _dbSet
			.Where(j => j.IsFeatured)
			.ExecuteUpdateAsync(setter => setter.SetProperty(j => j.IsFeatured, false), cancellationToken);
	}
	public IQueryable<Job> GetQueryable()
	{
		return _dbSet.AsQueryable();
	}
	public async Task<List<MonthlyJobStat>> GetJobsByMonthAsync(int year, CancellationToken cancellationToken = default)
	{
		var startDate = new DateTime(year, 1, 1);
		var endDate = new DateTime(year, 12, 31, 23, 59, 59);

		return await _context.Jobs
			.Where(j => j.CreatedAt >= startDate && j.CreatedAt <= endDate && !j.IsDeleted)
			.GroupBy(j => j.CreatedAt.Month)
			.Select(g => new MonthlyJobStat  // ⭐ Domain class
			{
				Month = g.Key,
				Total = g.Count()
			})
			.ToListAsync(cancellationToken);
	}
}