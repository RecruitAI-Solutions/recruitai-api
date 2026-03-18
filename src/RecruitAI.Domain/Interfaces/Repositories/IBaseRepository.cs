using System.Linq.Expressions;

namespace RecruitAI.Domain.Interfaces.Repositories
{
	public interface IBaseRepository<T> where T : class
	{
		// Query với CancellationToken
		Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
		Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
		Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
		Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
		Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
		Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
		IQueryable<T> Query();

		// Command (không cần CancellationToken vì chưa execute)
		Task AddAsync(T entity, CancellationToken cancellationToken = default);
		Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
		void Update(T entity);
		void UpdateRange(IEnumerable<T> entities);
		void Remove(T entity);
		void RemoveRange(IEnumerable<T> entities);
	}
}
