using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using System.Text.Json;

namespace RecruitAI.Application.Queries.Applications;

public class GetJobApplicationsQueryHandler : IRequestHandler<GetJobApplicationsQuery, PaginationResponseDto<JobApplicationDto>>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMessageService _msg;

	public GetJobApplicationsQueryHandler(IUnitOfWork unitOfWork, IMessageService msg)
	{
		_unitOfWork = unitOfWork;
		_msg = msg;
	}

	public async Task<PaginationResponseDto<JobApplicationDto>> Handle(GetJobApplicationsQuery request, CancellationToken cancellationToken)
	{
		// Check if job exists and user is owner
		var job = await _unitOfWork.Jobs.GetByIdAsync(request.JobId, cancellationToken);
		if (job == null)
			_msg.Throw(ErrorCode.ResourceNotFound, "JobNotFound");

		if (job.RecruiterId != request.RecruiterId)
			_msg.Throw(ErrorCode.Forbidden, "NoPermissionToViewApplications");

		var result = await _unitOfWork.JobApplications.GetByJobIdWithFilterAsync(
			request.JobId, request.Page, request.PageSize,
			request.Status, request.MinMatch, request.SortBy, request.SortOrder, cancellationToken);

		var items = new List<JobApplicationDto>();

		foreach (var app in result.Items)
		{
			var matchedSkills = new List<string>();
			var missingSkills = new List<string>();

			if (app.Match != null)
			{
				try
				{
					var matched = JsonSerializer.Deserialize<List<SkillMatchDetailDto>>(app.Match.MatchedSkillsJson);
					var missing = JsonSerializer.Deserialize<List<SkillMatchDetailDto>>(app.Match.MissingSkillsJson);

					matchedSkills = matched?.Select(s => s.Name).ToList() ?? new();
					missingSkills = missing?.Select(s => s.Name).ToList() ?? new();
				}
				catch { }
			}

			items.Add(new JobApplicationDto
			{
				ApplicationId = app.Id,
				CvId = app.CVId,
				CandidateName = app.CV?.User?.FullName ?? "Unknown",
				CandidateEmail = app.CV?.User?.Email ?? "Unknown",
				MatchPercentage = app.Match?.MatchPercentage ?? 0,
				MatchedSkillCount = app.Match?.MatchedSkillCount ?? 0,
				RequiredSkillCount = app.Match?.RequiredSkillCount ?? 0,
				MatchedSkills = matchedSkills,
				MissingSkills = missingSkills,
				Status = app.Status,
				AppliedAt = app.AppliedAt,
				CvDownloadUrl = $"/api/v1/cv/{app.CVId}/download"
			});
		}

		return new PaginationResponseDto<JobApplicationDto>
		{
			Data = items,
			Total = result.Total,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}
}