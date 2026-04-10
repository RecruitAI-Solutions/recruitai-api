namespace RecruitAI.Application.DTOs.AI
{
	public class MatchScoreDto
	{
		public int TotalScore { get; set; }
		public int SkillMatch { get; set; }
		public int ExperienceMatch { get; set; }
		public int SalaryMatch { get; set; }
		public int LocationMatch { get; set; }
		public List<string> SkillsMatch { get; set; } = new();
		public List<string> SkillsMissing { get; set; } = new();
		public bool ExperienceMatchBool { get; set; }
		public bool SalaryMatchBool { get; set; }
		public bool LocationMatchBool { get; set; }
		public string Reason { get; set; } = string.Empty;
	}
}