namespace RecruitAI.Application.DTOs.Responses.Skill
{
	public class SkillSuggestionDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Category { get; set; }
	}
}