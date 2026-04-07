using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetUsersListQuery : IRequest<AdminUserListResponseDto>
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string? Role { get; set; }
		public int? Status { get; set; }
		public string? Keyword { get; set; }
		public string SortBy { get; set; } = "createdAt";
		public string SortOrder { get; set; } = "desc";
	}
}