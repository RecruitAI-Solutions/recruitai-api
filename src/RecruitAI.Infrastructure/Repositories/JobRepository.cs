using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.DTOs;
using RecruitAI.Domain.Common.Jobs;
using RecruitAI.Domain.Common.Paginations;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Interfaces;
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
			.Include(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
			.FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted, cancellationToken);
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

		if (filter.EmploymentType.HasValue)
		{
			query = query.Where(j => j.EmploymentType == filter.EmploymentType);
		}

		if (filter.ExperienceLevel.HasValue)
		{
			query = query.Where(j => j.ExperienceLevel == filter.ExperienceLevel);
		}

		if (!string.IsNullOrWhiteSpace(filter.Skill))
		{
			query = query.Where(j => j.JobSkills.Any(js =>
				js.Skill.Name.Contains(filter.Skill) ||
				(js.Skill.Aliases != null && js.Skill.Aliases.Contains(filter.Skill))));
		}

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

		if (filter.EmploymentType.HasValue)
			query = query.Where(j => j.EmploymentType == filter.EmploymentType.Value);

		if (filter.ExperienceLevel.HasValue)
			query = query.Where(j => j.ExperienceLevel == filter.ExperienceLevel.Value);
		if (!string.IsNullOrWhiteSpace(filter.Skill))
		{
			query = query.Where(j => j.JobSkills.Any(js =>
				js.Skill.Name.Contains(filter.Skill) ||
				(js.Skill.Aliases != null && js.Skill.Aliases.Contains(filter.Skill))));
		}

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

		if (filter.EmploymentType.HasValue)
			query = query.Where(j => j.EmploymentType == filter.EmploymentType);

		if (filter.ExperienceLevel.HasValue)
			query = query.Where(j => j.ExperienceLevel == filter.ExperienceLevel);
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
}