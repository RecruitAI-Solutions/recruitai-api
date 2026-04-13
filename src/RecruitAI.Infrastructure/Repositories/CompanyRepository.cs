using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Common.Companies;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories
{
	public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
	{
		public CompanyRepository(RecruitDevContext context) : base(context)
		{
		}

		public async Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
		}

		public async Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
		}

		public async Task<List<Company>> SuggestAsync(string keyword, int limit = 10, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(keyword))
				return new List<Company>();

			return await _dbSet
				.Where(c => c.Name.Contains(keyword) || (c.Slug != null && c.Slug.Contains(keyword)))
				.OrderBy(c => c.Name)
				.Take(limit)
				.ToListAsync(cancellationToken);
		}

		public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			return await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
		}

		public async Task<Company?> GetByIdWithJobsAsync(Guid id, CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Include(c => c.Jobs)
				.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
		}

		public async Task<(List<Company> Items, int Total)> GetCompaniesAsync(CompanyFilter filter, CancellationToken cancellationToken = default)
		{
			var query = _dbSet.AsQueryable();

			if (!string.IsNullOrWhiteSpace(filter.Keyword))
			{
				query = query.Where(c => c.Name.Contains(filter.Keyword));
			}

			var total = await query.CountAsync(cancellationToken);

			// Sorting
			query = filter.SortBy?.ToLower() switch
			{
				"name" => filter.SortOrder == "asc" ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
				"createdat" => filter.SortOrder == "asc" ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
				_ => query.OrderBy(c => c.Name)
			};

			var items = await query
				.Skip((filter.Page - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync(cancellationToken);

			return (items, total);
		}
	}
}