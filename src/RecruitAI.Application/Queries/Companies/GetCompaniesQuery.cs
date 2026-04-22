using AutoMapper;
using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Companies;
using RecruitAI.Application.DTOs.Responses.Companies;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Common.Companies;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Companies;

public class GetCompaniesQuery : IRequest<PaginationResponseDto<CompanyResponseDto>>
{
	public CompanyFilterDto Filter { get; set; } = new();
}

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PaginationResponseDto<CompanyResponseDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;

	public GetCompaniesQueryHandler(IUnitOfWork uow, IMapper mapper)
	{
		_uow = uow;
		_mapper = mapper;
	}

	public async Task<PaginationResponseDto<CompanyResponseDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
	{
		var domainFilter = new CompanyFilter
		{
			Keyword = request.Filter.Keyword,
			SortBy = request.Filter.SortBy,
			SortOrder = request.Filter.SortOrder,
			Page = request.Filter.Page,
			PageSize = request.Filter.PageSize
		};

		(List<Company> companies, int total) = await _uow.Companies.GetCompaniesAsync(domainFilter, cancellationToken);

		var items = _mapper.Map<List<CompanyResponseDto>>(companies);

		foreach (var item in items)
		{
			var company = companies.First(c => c.Id == item.Id);
			item.TotalJobs = company.Jobs?.Count ?? 0;
		}

		return new PaginationResponseDto<CompanyResponseDto>
		{
			Data = items,
			Total = total,
			Page = request.Filter.Page,
			PageSize = request.Filter.PageSize
		};
	}
}