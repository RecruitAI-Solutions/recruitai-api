using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Mappings;
using RecruitAI.Domain.Common;
using RecruitAI.Domain.Interfaces;
using RecruitAI.Domain.Interfaces.Repositories;

namespace RecruitAI.Application.Queries.Jobs;

public class GetDeletedJobsQuery : IRequest<PaginationResponseDto<JobListDto>>
{
	public JobFilterDto Filter { get; set; } = new();
}

public class GetDeletedJobsQueryHandler : IRequestHandler<GetDeletedJobsQuery, PaginationResponseDto<JobListDto>>
{
	private readonly IJobRepository _jobRepository;
	private readonly IMapper _mapper;
	private readonly ILogger<GetDeletedJobsQueryHandler> _logger;

	public GetDeletedJobsQueryHandler(
		IJobRepository jobRepository,
		IMapper mapper,
		ILogger<GetDeletedJobsQueryHandler> logger)
	{
		_jobRepository = jobRepository;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<PaginationResponseDto<JobListDto>> Handle(GetDeletedJobsQuery request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Getting deleted jobs with filter: {@Filter}", request.Filter);

			// Map từ DTO sang Domain Filter
			var domainFilter = request.Filter.ToDomainFilter();

			// Lấy jobs đã xóa (IsDeleted = true)
			var result = await _jobRepository.GetDeletedJobsAsync(domainFilter, cancellationToken);

			var jobDtos = _mapper.Map<List<JobListDto>>(result.Items);

			return new PaginationResponseDto<JobListDto>
			{
				Data = jobDtos,
				Total = result.Total,
				Page = domainFilter.Page,
				PageSize = domainFilter.PageSize
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting deleted jobs");
			throw;
		}
	}
}