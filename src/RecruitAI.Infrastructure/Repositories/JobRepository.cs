using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.DTOs;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces;
using RecruitAI.Domain.Common;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
	private readonly RecruitDevContext _context;

	public JobRepository(RecruitDevContext context)
	{
		_context = context;
	}

	public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _context.Jobs
			.Include(j => j.Recruiter)
			.FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted, cancellationToken);
	}

	public async Task<PagedResult<Job>> GetJobsAsync(
		JobFilter filter,
		CancellationToken cancellationToken = default)
	{
		var query = _context.Jobs
			.Include(j => j.Recruiter)
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
			query = query.Where(j => j.Skills.Contains(filter.Skill));
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
		await _context.Jobs.AddAsync(job, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
	{
		_context.Jobs.Update(job);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var job = await GetByIdAsync(id, cancellationToken);
		if (job != null)
		{
			job.IsDeleted = true;
			_context.Jobs.Update(job);
			await _context.SaveChangesAsync(cancellationToken);
		}
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _context.Jobs
			.AnyAsync(j => j.Id == id && !j.IsDeleted, cancellationToken);
	}

	public async Task<bool> IsOwnerAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default)
	{
		return await _context.Jobs
			.AnyAsync(j => j.Id == jobId && j.RecruiterId == userId && !j.IsDeleted, cancellationToken);
	}

	public async Task<(List<Job> Items, int Total)> GetJobsByRecruiterAsync(
	Guid recruiterId,
	JobFilter filter,
	CancellationToken cancellationToken = default)
	{
		var query = _context.Jobs
			.Include(j => j.Recruiter)
			.Where(j => !j.IsDeleted && j.RecruiterId == recruiterId);

		// Apply filters (giống như GetJobsAsync)
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
			query = query.Where(j => j.Skills.Contains(filter.Skill));

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
}