using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Mappings;

namespace RecruitAI.Application.Queries.Jobs;

public class GetJobsQuery : IRequest<PaginationResponseDto<JobListDto>>
{
	public JobFilterDto Filter { get; set; } = new();
}

public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, PaginationResponseDto<JobListDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;
	private readonly ILogger<GetJobsQueryHandler> _logger;

	public GetJobsQueryHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<GetJobsQueryHandler> logger)
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<PaginationResponseDto<JobListDto>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Getting jobs with filter: {@Filter}", request.Filter);

			var domainFilter = request.Filter.ToDomainFilter();

			var result = await _uow.Jobs.GetJobsAsync(filter: domainFilter, cancellationToken: cancellationToken);
			var jobDtos = _mapper.Map<List<JobListDto>>(result.Items);

			return new PaginationResponseDto<JobListDto>
			{
				Data = jobDtos,
				Total = result.Total,
				Page = request.Filter.Page,
				PageSize = request.Filter.PageSize
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting jobs");
			throw;
		}
	}
}