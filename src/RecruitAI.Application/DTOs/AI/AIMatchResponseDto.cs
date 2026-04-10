namespace RecruitAI.Application.DTOs.AI
{
	public class AIMatchResponseDto
	{
		public int MatchPercentage { get; set; }
		public int SkillMatch { get; set; }
		public int ExperienceMatch { get; set; }
		public int SalaryMatch { get; set; }
		public int LocationMatch { get; set; }
		public List<string> MatchedSkills { get; set; } = new();
		public List<string> MissingSkills { get; set; } = new();
		public string AiReason { get; set; } = string.Empty;
		public bool UsedAI { get; set; }
		public bool UsedCache { get; set; }
	}
}