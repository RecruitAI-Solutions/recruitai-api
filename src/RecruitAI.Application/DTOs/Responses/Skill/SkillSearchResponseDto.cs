using System.Collections.Generic;

namespace RecruitAI.Application.DTOs.Responses.Skill
{
	public class SkillSearchResponseDto
	{
		public List<SkillResponseDto> Items { get; set; } = new();
		public int TotalCount { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
	}
}