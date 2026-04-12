using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Applications;

public class MyApplicationDto
{
	public Guid ApplicationId { get; set; }
	public Guid JobId { get; set; }
	public string JobTitle { get; set; } = string.Empty;
	public string Company { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
	public int MatchPercentage { get; set; }
    public JobApplicationStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
	public string? StatusDisplay { get; set; }
	public DateTime AppliedAt { get; set; }
	public DateTime? ReviewedAt { get; set; }
}