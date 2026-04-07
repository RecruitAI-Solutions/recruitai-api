using RecruitAI.Application.DTOs.Common;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Requests.Admin
{
	public class GetUsersListRequestDto : PaginationRequestDto
	{
		public UserRole? Role { get; set; }
		public UserStatus? Status { get; set; }
		public string? Keyword { get; set; }
		public string SortBy { get; set; } = "createdAt";
		public string SortOrder { get; set; } = "desc";
	}
}