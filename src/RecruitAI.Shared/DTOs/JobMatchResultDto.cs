// RecruitAI.Shared/DTOs/JobMatchResultDto.cs
namespace RecruitAI.Shared.DTOs
{
	public class JobMatchResultDto
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string CompanyName { get; set; } = string.Empty;
		public string Location { get; set; } = string.Empty;
		public string SalaryDisplay { get; set; } = string.Empty;
		public string EmploymentType { get; set; } = string.Empty;
		public string ExperienceLevel { get; set; } = string.Empty;
		public int MatchedSkillCount { get; set; }
		public int TotalRequiredSkills { get; set; }
		public int MatchPercentage { get; set; }
		public List<string> MatchedSkills { get; set; } = new();
		public List<string> MissingSkills { get; set; } = new();
		public DateTime CreatedAt { get; set; }
		public int Views { get; set; }
		public int Applications { get; set; }
	}
}