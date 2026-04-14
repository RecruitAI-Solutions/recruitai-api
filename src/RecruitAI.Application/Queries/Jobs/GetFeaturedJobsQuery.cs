using AutoMapper;
using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Interfaces;

namespace RecruitAI.Application.Queries.Jobs;

public class GetFeaturedJobsQuery : IRequest<PaginationResponseDto<JobListDto>>
{
	public int Limit { get; set; } = 10;
}

public class GetFeaturedJobsQueryHandler : IRequestHandler<GetFeaturedJobsQuery, PaginationResponseDto<JobListDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;

	public GetFeaturedJobsQueryHandler(IUnitOfWork uow, IMapper mapper)
	{
		_uow = uow;
		_mapper = mapper;
	}

	public async Task<PaginationResponseDto<JobListDto>> Handle(GetFeaturedJobsQuery request, CancellationToken cancellationToken)
	{
		var jobs = await _uow.Jobs.GetFeaturedJobsAsync(request.Limit, cancellationToken);

		var items = _mapper.Map<List<JobListDto>>(jobs);

		return new PaginationResponseDto<JobListDto>
		{
			Data = items,
			Total = jobs.Count,
			Page = 1,
			PageSize = request.Limit
		};
	}
}