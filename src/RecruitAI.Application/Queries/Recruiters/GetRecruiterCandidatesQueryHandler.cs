using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Recruiters;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Shared.Interfaces;
using System.Text.Json;

namespace RecruitAI.Application.Queries.Recruiters;

public class GetRecruiterCandidatesQueryHandler : IRequestHandler<GetRecruiterCandidatesQuery, PaginationResponseDto<RecruiterCandidateDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMessageService _msg;

	public GetRecruiterCandidatesQueryHandler(IUnitOfWork uow, IMessageService msg)
	{
		_uow = uow;
		_msg = msg;
	}

	public async Task<PaginationResponseDto<RecruiterCandidateDto>> Handle(GetRecruiterCandidatesQuery request, CancellationToken cancellationToken)
	{
		// Lấy tất cả job của recruiter
		var recruiterJobIds = await _uow.Jobs.GetQueryable()
			.Where(j => j.RecruiterId == request.RecruiterId && !j.IsDeleted)
			.Select(j => j.Id)
			.ToListAsync(cancellationToken);

		if (!recruiterJobIds.Any())
		{
			return new PaginationResponseDto<RecruiterCandidateDto>
			{
				Data = new List<RecruiterCandidateDto>(),
				Total = 0,
				Page = request.Page,
				PageSize = request.PageSize
			};
		}

		// Build query
		var query = _uow.JobApplications.GetQueryable()
			.Include(x => x.Job)
			.Include(x => x.CV)
				.ThenInclude(cv => cv.User)
			.Include(x => x.Match)
			.Where(x => recruiterJobIds.Contains(x.JobId));

		// Filter by job
		if (request.JobId.HasValue)
		{
			query = query.Where(x => x.JobId == request.JobId.Value);
		}

		// Filter by status
		if (request.Status.HasValue)
		{
			query = query.Where(x => x.Status == request.Status.Value);
		}

		// Filter by min match
		if (request.MinMatch.HasValue)
		{
			query = query.Where(x => x.Match != null && x.Match.MatchPercentage >= request.MinMatch.Value);
		}

		// Search by candidate name, email or job title
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
			"matchpercentage" => request.SortOrder == "asc"
				? query.OrderBy(x => x.Match != null ? x.Match.MatchPercentage : 0)
				: query.OrderByDescending(x => x.Match != null ? x.Match.MatchPercentage : 0),
			"candidateName" => request.SortOrder == "asc"
				? query.OrderBy(x => x.CV != null && x.CV.User != null ? x.CV.User.FullName : "")
				: query.OrderByDescending(x => x.CV != null && x.CV.User != null ? x.CV.User.FullName : ""),
			"jobtitle" => request.SortOrder == "asc"
				? query.OrderBy(x => x.Job != null ? x.Job.Title : "")
				: query.OrderByDescending(x => x.Job != null ? x.Job.Title : ""),
			"status" => request.SortOrder == "asc"
				? query.OrderBy(x => x.Status)
				: query.OrderByDescending(x => x.Status),
			_ => request.SortOrder == "asc"
				? query.OrderBy(x => x.AppliedAt)
				: query.OrderByDescending(x => x.AppliedAt)
		};

		var distinctQuery = query.Distinct();

		var total = await distinctQuery.CountAsync(cancellationToken);

		var items = await distinctQuery
			.Skip((request.Page - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToListAsync(cancellationToken);

		var result = items.Select(app => new RecruiterCandidateDto
		{
			CandidateId = app.CV?.UserId ?? Guid.Empty,
			CandidateName = app.CV?.User?.FullName ?? "Unknown",
			CandidateEmail = app.CV?.User?.Email ?? "Unknown",
			CandidatePhone = app.CV?.User?.PhoneNumber,
			AvatarUrl = app.CV?.User?.AvatarUrl,
			JobId = app.JobId,
			JobTitle = app.Job?.Title ?? "Unknown",
			JobLocation = app.Job?.Location ?? "Unknown",
			ApplicationId = app.Id,
			MatchPercentage = app.Match?.MatchPercentage ?? 0,
			MatchedSkills = GetSkillNamesFromJson(app.Match?.MatchedSkillsJson),
			MissingSkills = GetSkillNamesFromJson(app.Match?.MissingSkillsJson),
			Status = app.Status,
			StatusName = app.Status.ToString(),
			StatusDisplay = _msg.Get($"ApplicationStatus.{app.Status}"),
			AppliedAt = app.AppliedAt,
			ReviewedAt = app.ReviewedAt,
			Notes = app.Notes
		}).ToList();

		return new PaginationResponseDto<RecruiterCandidateDto>
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