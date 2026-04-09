using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Common.CVs;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories;

public class CVRepository : BaseRepository<CV>, ICVRepository
{
	public CVRepository(RecruitDevContext context) : base(context)
	{
	}

	public async Task<CV?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
	}

	public async Task<IEnumerable<CV>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(c => c.UserId == userId)
			.OrderByDescending(c => c.UploadedAt)
			.ToListAsync(cancellationToken);
	}

	public async Task<(IEnumerable<CV> Items, int Total)> GetUserCVsAsync(
		Guid userId,
		CVFilter filter,
		CancellationToken cancellationToken = default)
	{
		// Bắt đầu query với điều kiện userId
		var query = _dbSet
			.Where(c => c.UserId == userId);

		// Filter by status
		if (filter.Status.HasValue)
		{
			query = query.Where(c => c.Status == filter.Status.Value);
		}

		// Filter by date range
		if (filter.FromDate.HasValue)
		{
			var fromDateUtc = filter.FromDate.Value.ToUniversalTime();
			query = query.Where(c => c.UploadedAt >= fromDateUtc);
		}

		if (filter.ToDate.HasValue)
		{
			var toDateUtc = filter.ToDate.Value.ToUniversalTime().Date.AddDays(1).AddTicks(-1);
			query = query.Where(c => c.UploadedAt <= toDateUtc);
		}

		// Filter by file name
		if (!string.IsNullOrWhiteSpace(filter.FileName))
		{
			query = query.Where(c => c.FileName.Contains(filter.FileName));
		}

		// Get total count before pagination
		var total = await query.CountAsync(cancellationToken);

		// Apply sorting
		query = filter.SortBy?.ToLower() switch
		{
			"filename" => filter.SortOrder?.ToLower() == "asc"
				? query.OrderBy(c => c.FileName)
				: query.OrderByDescending(c => c.FileName),
			"filesize" => filter.SortOrder?.ToLower() == "asc"
				? query.OrderBy(c => c.FileSize)
				: query.OrderByDescending(c => c.FileSize),
			"status" => filter.SortOrder?.ToLower() == "asc"
				? query.OrderBy(c => c.Status)
				: query.OrderByDescending(c => c.Status),
			_ => filter.SortOrder?.ToLower() == "asc"
				? query.OrderBy(c => c.UploadedAt)
				: query.OrderByDescending(c => c.UploadedAt)
		};

		// Apply pagination
		var items = await query
			.Skip((filter.Page - 1) * filter.PageSize)
			.Take(filter.PageSize)
			.ToListAsync(cancellationToken);

		return (items, total);
	}

	public async Task AddAsync(CV cv, CancellationToken cancellationToken = default)
	{
		await _dbSet.AddAsync(cv, cancellationToken);
	}

	public async Task UpdateAsync(CV cv, CancellationToken cancellationToken = default)
	{
		_dbSet.Update(cv);
		await Task.CompletedTask;
	}

	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var cv = await GetByIdAsync(id, cancellationToken);
		if (cv != null)
		{
			_dbSet.Remove(cv);

			// Xóa file vật lý
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cv.FilePath);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
	}
	public async Task<Dictionary<CVStatus, int>> CountCVsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
	{
		var query = _dbSet.Where(c => !c.IsDeleted);

		if (fromDate.HasValue)
			query = query.Where(c => c.UploadedAt >= fromDate.Value);

		if (toDate.HasValue)
		{
			var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
			query = query.Where(c => c.UploadedAt <= toDateEnd);
		}

		var items = await query
			.GroupBy(c => c.Status)
			.Select(g => new { Status = g.Key, Count = g.Count() })
			.ToListAsync(cancellationToken);

		return items.ToDictionary(x => x.Status, x => x.Count);
	}

	public async Task<int[]> CountCVsByDayAsync(int days, DateTime? endDate = null, CancellationToken cancellationToken = default)
	{
		var end = endDate ?? DateTime.UtcNow;
		var startDate = end.AddDays(-days + 1).Date;
		var result = new int[days];

		var items = await _dbSet
			.Where(c => c.UploadedAt >= startDate && !c.IsDeleted)
			.GroupBy(c => c.UploadedAt.Date)
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