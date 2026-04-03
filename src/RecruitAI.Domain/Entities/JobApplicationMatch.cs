namespace RecruitAI.Domain.Entities;

public class JobApplicationMatch
{
	public Guid Id { get; set; }
	public Guid ApplicationId { get; set; }
	public int MatchPercentage { get; set; }
	public int RequiredSkillCount { get; set; }
	public int MatchedSkillCount { get; set; }
	public string MatchedSkillsJson { get; set; } = string.Empty;
	public string MissingSkillsJson { get; set; } = string.Empty;
	public DateTime CalculatedAt { get; set; }

	// Navigation
	public virtual JobApplication Application { get; set; } = null!;
}