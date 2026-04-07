using RecruitAI.Application.DTOs.Common;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Jobs;

public class ApplyJobResponseDto
{
	public Guid ApplicationId { get; set; }
	public Guid JobId { get; set; }
	public string JobTitle { get; set; } = string.Empty;
	public Guid CvId { get; set; }
	public string CvName { get; set; } = string.Empty;
	public int MatchPercentage { get; set; }
	public int MatchedSkillCount { get; set; }
	public int RequiredSkillCount { get; set; }
	public List<SkillMatchDetailDto> MatchedSkills { get; set; } = new();
	public List<SkillMatchDetailDto> MissingSkills { get; set; } = new();
	public JobApplicationStatus Status { get; set; }
	public DateTime AppliedAt { get; set; }
}