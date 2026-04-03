using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Domain.Entities;
using RecruitAI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecruitAI.Infrastructure.Repositories
{
	public class SkillRepository : BaseRepository<Skill>, ISkillRepository
	{
		public SkillRepository(RecruitDevContext context) : base(context)
		{
		}

		public async Task<Skill?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
		}

		public async Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default)
		{
			skill.CreatedAt = DateTime.UtcNow;
			await _dbSet.AddAsync(skill, cancellationToken);
			return skill;
		}

		public async Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
		{
			skill.UpdatedAt = DateTime.UtcNow;
			_dbSet.Update(skill);
			await Task.CompletedTask;
		}

		public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
		{
			var skill = await GetByIdAsync(id, cancellationToken);
			if (skill != null)
			{
				skill.IsActive = false;
				skill.UpdatedAt = DateTime.UtcNow;
				_dbSet.Update(skill);
			}
		}

		public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
		{
			return await _dbSet.AnyAsync(s => s.Id == id, cancellationToken);
		}

		public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			return await _dbSet.AnyAsync(s => s.Name.ToLower() == name.ToLower(), cancellationToken);
		}

		public async Task<(IEnumerable<Skill> Items, int TotalCount)> SearchAsync(
			string? keyword = null,
			string? category = null,
			bool? isActive = null,
			int page = 1,
			int pageSize = 20,
			string sortBy = "name",
			string sortOrder = "asc",
			CancellationToken cancellationToken = default)
		{
			var query = _dbSet.AsQueryable();

			// Apply filters
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				keyword = keyword.ToLower();
				query = query.Where(s =>
					s.Name.ToLower().Contains(keyword) ||
					(s.Aliases != null && s.Aliases.ToLower().Contains(keyword)));
			}

			if (!string.IsNullOrWhiteSpace(category))
				query = query.Where(s => s.Category == category);

			if (isActive.HasValue)
				query = query.Where(s => s.IsActive == isActive.Value);

			var totalCount = await query.CountAsync(cancellationToken);

			// Apply sorting
			query = sortBy.ToLower() switch
			{
				"category" => sortOrder.ToLower() == "desc"
					? query.OrderByDescending(s => s.Category).ThenBy(s => s.Name)
					: query.OrderBy(s => s.Category).ThenBy(s => s.Name),
				"createdat" => sortOrder.ToLower() == "desc"
					? query.OrderByDescending(s => s.CreatedAt)
					: query.OrderBy(s => s.CreatedAt),
				_ => sortOrder.ToLower() == "desc"
					? query.OrderByDescending(s => s.Name)
					: query.OrderBy(s => s.Name)
			};

			// Apply pagination
			var items = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync(cancellationToken);

			return (items, totalCount);
		}

		public async Task<IEnumerable<Skill>> SuggestAsync(string keyword, int limit = 10, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(keyword))
				return new List<Skill>();

			keyword = keyword.ToLower();

			return await _dbSet
				.Where(s => s.IsActive &&
					(s.Name.ToLower().Contains(keyword) ||
					 (s.Aliases != null && s.Aliases.ToLower().Contains(keyword))))
				.OrderBy(s => s.Name)
				.Take(limit)
				.ToListAsync(cancellationToken);
		}

		public async Task<IEnumerable<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Where(s => s.IsActive && s.Category != null)
				.Select(s => s.Category!)
				.Distinct()
				.OrderBy(c => c)
				.ToListAsync(cancellationToken);
		}
		public async Task<IEnumerable<Skill>> GetAllActiveAsync(CancellationToken cancellationToken = default)
		{
			return await _dbSet
				.Where(s => s.IsActive)
				.OrderBy(s => s.Name)
				.ToListAsync(cancellationToken);
		}

	}
}