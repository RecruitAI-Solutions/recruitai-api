using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Applications;

public class JobApplicationDto
{
	public Guid ApplicationId { get; set; }
	public Guid CvId { get; set; }
	public string CandidateName { get; set; } = string.Empty;
	public string CandidateEmail { get; set; } = string.Empty;
	public int MatchPercentage { get; set; }
	public int MatchedSkillCount { get; set; }
	public int RequiredSkillCount { get; set; }
	public List<string> MatchedSkills { get; set; } = new();
	public List<string> MissingSkills { get; set; } = new();
    public JobApplicationStatus Status { get; set; }
	public string StatusName { get; set; } = string.Empty;
	public string? StatusDisplay { get; set; }
	public DateTime AppliedAt { get; set; }
	public string CvDownloadUrl { get; set; } = string.Empty;
}