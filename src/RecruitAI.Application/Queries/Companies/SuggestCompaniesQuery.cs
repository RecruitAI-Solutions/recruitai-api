using AutoMapper;
using MediatR;
using RecruitAI.Application.DTOs.Companies;
using RecruitAI.Application.DTOs.Responses.Companies;
using RecruitAI.Application.Interfaces;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Companies;

public class SuggestCompaniesQuery : IRequest<List<CompanySuggestDto>>
{
	public string Keyword { get; set; } = string.Empty;
	public int Limit { get; set; } = 10;
}

public class SuggestCompaniesQueryHandler : IRequestHandler<SuggestCompaniesQuery, List<CompanySuggestDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;

	public SuggestCompaniesQueryHandler(IUnitOfWork uow, IMapper mapper)
	{
		_uow = uow;
		_mapper = mapper;
	}

	public async Task<List<CompanySuggestDto>> Handle(SuggestCompaniesQuery request, CancellationToken cancellationToken)
	{
		var companies = await _uow.Companies.SuggestAsync(request.Keyword, request.Limit, cancellationToken);
		return _mapper.Map<List<CompanySuggestDto>>(companies);
	}
}