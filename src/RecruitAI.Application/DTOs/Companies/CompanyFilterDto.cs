using RecruitAI.Application.DTOs.Common;

namespace RecruitAI.Application.DTOs.Companies
{
	public class CompanyFilterDto : PaginationRequestDto
	{
		public string? Keyword { get; set; }
		public string? SortBy { get; set; } = "name";
		public string SortOrder { get; set; } = "asc";
	}
}
