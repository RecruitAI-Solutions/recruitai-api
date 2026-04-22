using AutoMapper;
using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Jobs;
using RecruitAI.Application.Interfaces;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Jobs;

public class GetSavedJobsQuery : IRequest<PaginationResponseDto<SavedJobResponseDto>>
{
	public Guid UserId { get; set; }
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
}

public class GetSavedJobsQueryHandler : IRequestHandler<GetSavedJobsQuery, PaginationResponseDto<SavedJobResponseDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;

	public GetSavedJobsQueryHandler(IUnitOfWork uow, IMapper mapper)
	{
		_uow = uow;
		_mapper = mapper;
	}

	public async Task<PaginationResponseDto<SavedJobResponseDto>> Handle(GetSavedJobsQuery request, CancellationToken cancellationToken)
	{
		var (savedJobs, total) = await _uow.SavedJobs.GetSavedJobsByUserAsync(
			request.UserId, request.Page, request.PageSize, cancellationToken);

		var items = savedJobs.Select(s => new SavedJobResponseDto
		{
			JobId = s.JobId,
			JobTitle = s.Job?.Title ?? string.Empty,
			CompanyName = s.Job?.Company?.Name,
			CompanyLogo = s.Job?.Company?.Logo,
			Location = s.Job?.Location ?? string.Empty,
			SalaryMin = s.Job?.SalaryMin,
			SalaryMax = s.Job?.SalaryMax,
			SkillNames = s.Job?.JobSkills?.Where(js => js.Skill != null).Select(js => js.Skill.Name).ToList() ?? new(),
			SavedAt = s.SavedAt
		}).ToList();

		return new PaginationResponseDto<SavedJobResponseDto>
		{
			Data = items,
			Total = total,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}
}