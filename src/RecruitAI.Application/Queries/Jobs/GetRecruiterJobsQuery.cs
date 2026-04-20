using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Common.Jobs;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Interfaces;

namespace RecruitAI.Application.Queries.Jobs;

public class GetRecruiterJobsQuery : IRequest<PaginationResponseDto<JobListDto>>
{
	public Guid RecruiterId { get; set; }
	public JobFilterDto Filter { get; set; } = new();
}

public class GetRecruiterJobsQueryHandler : IRequestHandler<GetRecruiterJobsQuery, PaginationResponseDto<JobListDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;
	private readonly ILogger<GetRecruiterJobsQueryHandler> _logger;

	public GetRecruiterJobsQueryHandler(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<GetRecruiterJobsQueryHandler> logger)
	{
		_uow = uow;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<PaginationResponseDto<JobListDto>> Handle(GetRecruiterJobsQuery request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Getting jobs for recruiter {RecruiterId} with filter: {@Filter}",
				request.RecruiterId, request.Filter);

			// Tạo domain filter từ DTO
			var domainFilter = new JobFilter
			{
				Title = request.Filter.Title,
				Location = request.Filter.Location,
				MinSalary = request.Filter.MinSalary,
				MaxSalary = request.Filter.MaxSalary,
				EmploymentType = request.Filter.EmploymentType?.Select(et => (EmploymentType)et).ToList() ?? new(),
				ExperienceLevel = request.Filter.ExperienceLevel?.Select(el => (ExperienceLevel)el).ToList() ?? new(),
				Skill = request.Filter.Skill,
				Skills = request.Filter.Skills,
				MatchAllSkills = request.Filter.MatchAllSkills,
				SortBy = request.Filter.SortBy,
				SortOrder = request.Filter.SortOrder,
				Page = request.Filter.Page,
				PageSize = request.Filter.PageSize
			};

			// Lấy jobs của recruiter cụ thể
			var (jobs, total) = await _uow.Jobs.GetJobsByRecruiterAsync(
				recruiterId: request.RecruiterId,
				filter: domainFilter,
				cancellationToken: cancellationToken);

			var jobDtos = _mapper.Map<List<JobListDto>>(jobs);

			return new PaginationResponseDto<JobListDto>
			{
				Data = jobDtos,
				Total = total,
				Page = request.Filter.Page,
				PageSize = request.Filter.PageSize
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting jobs for recruiter {RecruiterId}", request.RecruiterId);
			throw;
		}
	}
}