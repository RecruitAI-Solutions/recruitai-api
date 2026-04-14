using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Interfaces.Services;

public interface ICompanyService
{
	Task<Company> CreateOrGetCompanyAsync(string companyName, string? website, string? address, Guid createdBy, CancellationToken cancellationToken = default);
}