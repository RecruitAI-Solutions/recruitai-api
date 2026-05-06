// GetRecruitersQuery.cs
using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Queries.Admin;

public class GetRecruitersQuery : IRequest<PaginationResponseDto<RecruiterListDto>>
{
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public string? Keyword { get; set; }
	public UserStatus? Status { get; set; }
	public string? SortBy { get; set; } = "createdAt";
	public string? SortOrder { get; set; } = "desc";
}