using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories;

public class CVRepository : ICVRepository
{
	private readonly RecruitDevContext _context;

	public CVRepository(RecruitDevContext context)
	{
		_context = context;
	}

	public async Task<CV?> GetByIdAsync(Guid id)
	{
		return await _context.CVs.FindAsync(id);
	}

	public async Task<IEnumerable<CV>> GetByUserIdAsync(Guid userId)
	{
		return await _context.CVs
			.Where(c => c.UserId == userId)
			.OrderByDescending(c => c.UploadedAt)
			.ToListAsync();
	}

	public async Task AddAsync(CV cv)
	{
		await _context.CVs.AddAsync(cv);
		await _context.SaveChangesAsync();
	}

	public async Task UpdateAsync(CV cv)
	{
		_context.CVs.Update(cv);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(Guid id)
	{
		var cv = await GetByIdAsync(id);
		if (cv != null)
		{
			_context.CVs.Remove(cv);
			await _context.SaveChangesAsync();

			// Xóa file vật lý
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cv.FilePath);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
	}
}