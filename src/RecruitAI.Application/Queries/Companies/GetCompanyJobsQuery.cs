using AutoMapper;
using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Entities;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Companies;

public class GetCompanyJobsQuery : IRequest<PaginationResponseDto<JobListDto>>
{
	public Guid CompanyId { get; set; }
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
}

public class GetCompanyJobsQueryHandler : IRequestHandler<GetCompanyJobsQuery, PaginationResponseDto<JobListDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;

	public GetCompanyJobsQueryHandler(IUnitOfWork uow, IMapper mapper)
	{
		_uow = uow;
		_mapper = mapper;
	}

	public async Task<PaginationResponseDto<JobListDto>> Handle(GetCompanyJobsQuery request, CancellationToken cancellationToken)
	{
		(List<Job> jobs, int total) = await _uow.Jobs.GetJobsByCompanyAsync(
		request.CompanyId,
		request.Page,
		request.PageSize,
		cancellationToken);

		var items = _mapper.Map<List<JobListDto>>(jobs);

		return new PaginationResponseDto<JobListDto>
		{
			Data = items,
			Total = total,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}
}