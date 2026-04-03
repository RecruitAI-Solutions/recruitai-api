using RecruitAI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecruitAI.Domain.Interfaces.Repositories
{
	public interface ISkillRepository
	{
		// CRUD cơ bản
		Task<Skill?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default);
		Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
		Task DeleteAsync(int id, CancellationToken cancellationToken = default); // Soft delete

		// Kiểm tra tồn tại
		Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
		Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

		// Tìm kiếm
		Task<(IEnumerable<Skill> Items, int TotalCount)> SearchAsync(
			string? keyword = null,
			string? category = null,
			bool? isActive = null,
			int page = 1,
			int pageSize = 20,
			string sortBy = "name",
			string sortOrder = "asc",
			CancellationToken cancellationToken = default);

		// Gợi ý
		Task<IEnumerable<Skill>> SuggestAsync(string keyword, int limit = 10, CancellationToken cancellationToken = default);

		// Categories
		Task<IEnumerable<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
		Task<IEnumerable<Skill>> GetAllActiveAsync(CancellationToken cancellationToken = default);

		Task<List<Skill>> GetAllAsync();

	}
}