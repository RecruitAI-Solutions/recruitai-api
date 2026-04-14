using AutoMapper;
using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Interfaces;

namespace RecruitAI.Application.Queries.Jobs;

public class GetSimilarJobsQuery : IRequest<PaginationResponseDto<JobListDto>>
{
	public Guid JobId { get; set; }
	public int Limit { get; set; } = 10;
}

public class GetSimilarJobsQueryHandler : IRequestHandler<GetSimilarJobsQuery, PaginationResponseDto<JobListDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;

	public GetSimilarJobsQueryHandler(IUnitOfWork uow, IMapper mapper)
	{
		_uow = uow;
		_mapper = mapper;
	}

	public async Task<PaginationResponseDto<JobListDto>> Handle(GetSimilarJobsQuery request, CancellationToken cancellationToken)
	{
		// Lấy job hiện tại để biết skill ids
		var currentJob = await _uow.Jobs.GetByIdAsync(request.JobId, cancellationToken);
		if (currentJob == null)
			return new PaginationResponseDto<JobListDto>();

		var skillIds = currentJob.JobSkills.Select(js => js.SkillId).ToList();

		// Tìm job có cùng skill (không bao gồm job hiện tại)
		var jobs = await _uow.Jobs.GetSimilarJobsAsync(skillIds, request.JobId, request.Limit, cancellationToken);

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