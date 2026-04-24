using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Applications;

public class GetMyApplicationsQueryHandler : IRequestHandler<GetMyApplicationsQuery, PaginationResponseDto<MyApplicationDto>>
{
	private readonly IUnitOfWork _unitOfWork;

	public GetMyApplicationsQueryHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<PaginationResponseDto<MyApplicationDto>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
	{
		// Build query
		var query = _unitOfWork.JobApplications.GetQueryable()
			.Include(x => x.Job)
				.ThenInclude(j => j.Company)
			.Include(x => x.Match)
			.Where(x => x.CV.UserId == request.UserId);

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

		// Filter by keyword (job title or company name)
		if (!string.IsNullOrWhiteSpace(request.Query))
		{
			query = query.Where(x =>
				(x.Job != null && x.Job.Title.Contains(request.Query)) ||
				(x.Job != null && x.Job.Company != null && x.Job.Company.Name.Contains(request.Query)));
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
			"appliedat" => request.SortOrder?.ToLower() == "asc"
				? query.OrderBy(x => x.AppliedAt)
				: query.OrderByDescending(x => x.AppliedAt),
			_ => query.OrderByDescending(x => x.AppliedAt)
		};

		// Get total count
		var total = await query.CountAsync(cancellationToken);

		// Get paged items
		var items = await query
			.Skip((request.Page - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToListAsync(cancellationToken);

		// Map to DTO
		var result = items.Select(app => new MyApplicationDto
		{
			ApplicationId = app.Id,
			JobId = app.JobId,
			JobTitle = app.Job?.Title ?? "Unknown",
			Company = app.Job?.Company?.Name ?? app.Job?.Department ?? "Unknown",
			Location = app.Job?.Location ?? "Unknown",
			MatchPercentage = app.Match?.MatchPercentage ?? 0,
			Status = app.Status,
			StatusName = app.Status.ToString(),
			StatusDisplay = app.Status.ToString(),
			AppliedAt = app.AppliedAt,
			ReviewedAt = app.ReviewedAt
		}).ToList();

		return new PaginationResponseDto<MyApplicationDto>
		{
			Data = result,
			Total = total,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}
}