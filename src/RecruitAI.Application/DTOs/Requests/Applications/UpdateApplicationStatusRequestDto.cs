using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Requests.Applications;

public class UpdateApplicationStatusRequestDto
{
	public JobApplicationStatus Status { get; set; }
	public string? Notes { get; set; }
}