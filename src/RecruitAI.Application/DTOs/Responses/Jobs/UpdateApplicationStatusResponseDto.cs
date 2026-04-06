using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Jobs;

public class UpdateApplicationStatusResponseDto
{
	public Guid ApplicationId { get; set; }
	public JobApplicationStatus Status { get; set; }
	public string? Notes { get; set; }
	public DateTime UpdatedAt { get; set; }
}