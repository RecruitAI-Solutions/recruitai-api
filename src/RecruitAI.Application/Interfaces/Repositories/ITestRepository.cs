using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Interfaces.Repositories
{
	public interface ITestRepository: IBaseRepository<Test>
	{
		Task<Test> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task<IEnumerable<Test>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
	}
}