using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Requests.Jobs;

public class UpdateApplicationStatusRequestDto
{
	public JobApplicationStatus Status { get; set; }
	public string? Notes { get; set; }
}