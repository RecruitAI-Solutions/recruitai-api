using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Common;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories;

public class CVRepository : BaseRepository<CV>, ICVRepository
{
	public CVRepository(RecruitDevContext context) : base(context)
	{
	}

	public async Task<CV?> GetByIdAsync(Guid id)
	{
		return await _dbSet
			.FirstOrDefaultAsync(c => c.Id == id);
	}

	public async Task<IEnumerable<CV>> GetByUserIdAsync(Guid userId)
	{
		return await _dbSet
			.Where(c => c.UserId == userId)
			.OrderByDescending(c => c.UploadedAt)
			.ToListAsync();
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

	public async Task AddAsync(CV cv)
	{
		await _dbSet.AddAsync(cv);
		
	}

	public async Task UpdateAsync(CV cv)
	{
		_dbSet.Update(cv);
		
	}

	public async Task DeleteAsync(Guid id)
	{
		var cv = await GetByIdAsync(id);
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
}