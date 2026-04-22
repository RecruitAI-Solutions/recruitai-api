using AutoMapper;
using MediatR;
using RecruitAI.Application.DTOs.Responses.Companies;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Companies;

public class GetCompanyDetailQuery : IRequest<CompanyResponseDto>
{
	public Guid Id { get; set; }
}

public class GetCompanyDetailQueryHandler : IRequestHandler<GetCompanyDetailQuery, CompanyResponseDto>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;

	public GetCompanyDetailQueryHandler(IUnitOfWork uow, IMapper mapper)
	{
		_uow = uow;
		_mapper = mapper;
	}

	public async Task<CompanyResponseDto> Handle(GetCompanyDetailQuery request, CancellationToken cancellationToken)
	{
		var company = await _uow.Companies.GetByIdWithJobsAsync(request.Id, cancellationToken);

		if (company == null)
			throw new BusinessException(ErrorCode.ResourceNotFound, "Company not found");

		var result = _mapper.Map<CompanyResponseDto>(company);
		result.TotalJobs = company.Jobs?.Count ?? 0;

		return result;
	}
}