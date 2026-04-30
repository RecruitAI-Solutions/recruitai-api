using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Recruiters;

public class RecruiterCandidateDto
{
	public Guid CandidateId { get; set; }
	public string CandidateName { get; set; } = string.Empty;
	public string CandidateEmail { get; set; } = string.Empty;
	public string? CandidatePhone { get; set; }
	public string? AvatarUrl { get; set; }

	public Guid JobId { get; set; }
	public string JobTitle { get; set; } = string.Empty;
	public string JobLocation { get; set; } = string.Empty;

	public Guid ApplicationId { get; set; }
	public int MatchPercentage { get; set; }
	public List<string> MatchedSkills { get; set; } = new();
	public List<string> MissingSkills { get; set; } = new();

	public JobApplicationStatus Status { get; set; }
	public string StatusName { get; set; } = string.Empty;
	public string StatusDisplay { get; set; } = string.Empty;

	public DateTime AppliedAt { get; set; }
	public DateTime? ReviewedAt { get; set; }
	public string? Notes { get; set; }
}