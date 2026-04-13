using RecruitAI.Domain.Common.Companies;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;

public interface ICompanyRepository : IBaseRepository<Company>
{
	Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
	Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
	Task<Company?> GetByIdWithJobsAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<Company>> SuggestAsync(string keyword, int limit = 10, CancellationToken cancellationToken = default);
	Task<(List<Company> Items, int Total)> GetCompaniesAsync(CompanyFilter filter, CancellationToken cancellationToken = default);
	Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}