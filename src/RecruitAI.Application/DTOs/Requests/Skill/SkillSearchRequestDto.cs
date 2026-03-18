namespace RecruitAI.Application.DTOs.Requests.Skill
{
	public class SkillSearchRequestDto
	{
		public string? Keyword { get; set; }
		public string? Category { get; set; }
		public bool? IsActive { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;
		public string SortBy { get; set; } = "name";
		public string SortOrder { get; set; } = "asc";
	}
}