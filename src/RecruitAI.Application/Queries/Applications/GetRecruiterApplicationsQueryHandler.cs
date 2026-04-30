// GetRecruiterApplicationsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Shared.Interfaces;
using System.Text.Json;

namespace RecruitAI.Application.Queries.Applications;

public class GetRecruiterApplicationsQueryHandler : IRequestHandler<GetRecruiterApplicationsQuery, PaginationResponseDto<RecruiterApplicationDto>>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMessageService _msg;

	public GetRecruiterApplicationsQueryHandler(IUnitOfWork unitOfWork, IMessageService msg)
	{
		_unitOfWork = unitOfWork;
		_msg = msg;
	}

	public async Task<PaginationResponseDto<RecruiterApplicationDto>> Handle(GetRecruiterApplicationsQuery request, CancellationToken cancellationToken)
	{
		// Build query - chỉ lấy job của recruiter này
		var query = _unitOfWork.JobApplications.GetQueryable()
			.Include(x => x.Job)
			.Include(x => x.CV)
				.ThenInclude(cv => cv.User)
			.Include(x => x.Match)
			.Where(x => x.Job != null && x.Job.RecruiterId == request.RecruiterId);

		// Filter by specific job
		if (request.JobId.HasValue)
		{
			query = query.Where(x => x.JobId == request.JobId.Value);
		}

		// Filter by status
		if (request.Status.HasValue)
		{
			query = query.Where(x => x.Status == request.Status.Value);
		}

		// Filter by date range
		if (request.FromDate.HasValue)
		{
			query = query.Where(x => x.AppliedAt >= request.FromDate.Value);
		}
		if (request.ToDate.HasValue)
		{
			var toDateEnd = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
			query = query.Where(x => x.AppliedAt <= toDateEnd);
		}

		// Filter by min match percentage
		if (request.MinMatch.HasValue)
		{
			query = query.Where(x => x.Match != null && x.Match.MatchPercentage >= request.MinMatch.Value);
		}

		// Filter by keyword (candidate name, email, job title)
		if (!string.IsNullOrWhiteSpace(request.Query))
		{
			query = query.Where(x =>
				(x.CV != null && x.CV.User != null && x.CV.User.FullName.Contains(request.Query)) ||
				(x.CV != null && x.CV.User != null && x.CV.User.Email.Contains(request.Query)) ||
				(x.Job != null && x.Job.Title.Contains(request.Query)));
		}

		// Sorting
		query = request.SortBy?.ToLower() switch
		{
			"matchpercentage" => request.SortOrder?.ToLower() == "asc"
				? query.OrderBy(x => x.Match != null ? x.Match.MatchPercentage : 0)
				: query.OrderByDescending(x => x.Match != null ? x.Match.MatchPercentage : 0),
			"jobtitle" => request.SortOrder?.ToLower() == "asc"
				? query.OrderBy(x => x.Job != null ? x.Job.Title : "")
				: query.OrderByDescending(x => x.Job != null ? x.Job.Title : ""),
			"candidatename" => request.SortOrder?.ToLower() == "asc"
				? query.OrderBy(x => x.CV != null && x.CV.User != null ? x.CV.User.FullName : "")
				: query.OrderByDescending(x => x.CV != null && x.CV.User != null ? x.CV.User.FullName : ""),
			"status" => request.SortOrder?.ToLower() == "asc"
				? query.OrderBy(x => x.Status)
				: query.OrderByDescending(x => x.Status),
			"appliedat" => request.SortOrder?.ToLower() == "asc"
				? query.OrderBy(x => x.AppliedAt)
				: query.OrderByDescending(x => x.AppliedAt),
			_ => query.OrderByDescending(x => x.AppliedAt)
		};

		var total = await query.CountAsync(cancellationToken);

		var items = await query
			.Skip((request.Page - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToListAsync(cancellationToken);

		var result = items.Select(app => new RecruiterApplicationDto
		{
			ApplicationId = app.Id,
			JobId = app.JobId,
			JobTitle = app.Job?.Title ?? "Unknown",
			JobLocation = app.Job?.Location ?? "Unknown",
			CandidateId = app.CV?.UserId ?? Guid.Empty,
			CandidateName = app.CV?.User?.FullName ?? "Unknown",
			CandidateEmail = app.CV?.User?.Email ?? "Unknown",
			CandidatePhone = app.CV?.User?.PhoneNumber,
			CvId = app.CVId,
			CvName = app.CV?.FileName ?? "Unknown",
			CvDownloadUrl = $"/api/v1/cv/{app.CVId}/download",
			MatchPercentage = app.Match?.MatchPercentage ?? 0,
			MatchedSkillCount = app.Match?.MatchedSkillCount ?? 0,
			RequiredSkillCount = app.Match?.RequiredSkillCount ?? 0,
			MatchedSkills = GetSkillNamesFromJson(app.Match?.MatchedSkillsJson),
			MissingSkills = GetSkillNamesFromJson(app.Match?.MissingSkillsJson),
			Status = app.Status,
			StatusName = app.Status.ToString(),
			StatusDisplay = _msg.Get($"ApplicationStatus.{app.Status}"),
			AppliedAt = app.AppliedAt,
			ReviewedAt = app.ReviewedAt,
			Notes = app.Notes
		}).ToList();

		return new PaginationResponseDto<RecruiterApplicationDto>
		{
			Data = result,
			Total = total,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}

	private List<string> GetSkillNamesFromJson(string? json)
	{
		if (string.IsNullOrEmpty(json)) return new List<string>();
		try
		{
			var skills = JsonSerializer.Deserialize<List<SkillMatchDetailDto>>(json);
			return skills?.Select(s => s.Name).ToList() ?? new List<string>();
		}
		catch
		{
			return new List<string>();
		}
	}
}