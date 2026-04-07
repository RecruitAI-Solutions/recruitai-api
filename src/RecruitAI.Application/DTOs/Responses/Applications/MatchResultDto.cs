namespace RecruitAI.Application.DTOs.Responses.Applications
{
	public class MatchResultDto
	{
		public int MatchPercentage { get; set; }
		public int MatchedSkillCount { get; set; }
		public int RequiredSkillCount { get; set; }
		public List<SkillDetailDto> MatchedSkills { get; set; } = new();
		public List<SkillDetailDto> MissingSkills { get; set; } = new();
		public DateTime CalculatedAt { get; set; }
	}

	public class SkillDetailDto
	{
		public int SkillId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Category { get; set; }
	}
}