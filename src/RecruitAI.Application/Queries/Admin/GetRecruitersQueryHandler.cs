// GetRecruitersQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.Application.Queries.Admin;

public class GetRecruitersQueryHandler : IRequestHandler<GetRecruitersQuery, PaginationResponseDto<RecruiterListDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMessageService _msg;

	public GetRecruitersQueryHandler(IUnitOfWork uow, IMessageService msg)
	{
		_uow = uow;
		_msg = msg;
	}

	public async Task<PaginationResponseDto<RecruiterListDto>> Handle(GetRecruitersQuery request, CancellationToken cancellationToken)
	{
		var query = _uow.Users.GetQueryable()
			.Where(u => u.Role == UserRole.RECRUITER);

		// Filter by status
		if (request.Status.HasValue)
		{
			query = query.Where(u => u.Status == request.Status.Value);
		}

		// Filter by keyword
		if (!string.IsNullOrWhiteSpace(request.Keyword))
		{
			query = query.Where(u =>
				u.Email.Contains(request.Keyword) ||
				u.FullName.Contains(request.Keyword) ||
				(u.PhoneNumber != null && u.PhoneNumber.Contains(request.Keyword)));
		}

		var total = await query.CountAsync(cancellationToken);

		// Sorting
		query = request.SortBy?.ToLower() switch
		{
			"fullname" => request.SortOrder == "asc"
				? query.OrderBy(u => u.FullName)
				: query.OrderByDescending(u => u.FullName),
			"email" => request.SortOrder == "asc"
				? query.OrderBy(u => u.Email)
				: query.OrderByDescending(u => u.Email),
			"status" => request.SortOrder == "asc"
				? query.OrderBy(u => u.Status)
				: query.OrderByDescending(u => u.Status),
			_ => request.SortOrder == "asc"
				? query.OrderBy(u => u.CreatedAt)
				: query.OrderByDescending(u => u.CreatedAt)
		};

		var items = await query
			.Skip((request.Page - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToListAsync(cancellationToken);

		// Get job count for each recruiter
		var recruiterIds = items.Select(r => r.Id).ToList();
		var jobCounts = await _uow.Jobs.GetQueryable()
			.Where(j => recruiterIds.Contains(j.RecruiterId) && !j.IsDeleted)
			.GroupBy(j => j.RecruiterId)
			.Select(g => new { RecruiterId = g.Key, Count = g.Count() })
			.ToDictionaryAsync(x => x.RecruiterId, x => x.Count, cancellationToken);

		var result = items.Select(r => new RecruiterListDto
		{
			Id = r.Id,
			Email = r.Email,
			FullName = r.FullName,
			PhoneNumber = r.PhoneNumber,
			AvatarUrl = r.AvatarUrl,
			Status = r.Status,
			StatusName = _msg.Get($"UserStatus.{r.Status}"),
			JobCount = jobCounts.GetValueOrDefault(r.Id, 0),
			CreatedAt = r.CreatedAt,
			LastLoginAt = r.LastLoginAt
		}).ToList();

		return new PaginationResponseDto<RecruiterListDto>
		{
			Data = result,
			Total = total,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}
}