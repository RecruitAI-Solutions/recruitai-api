using MediatR;
using RecruitAI.Application.DTOs.Responses.Jobs;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Commands.Jobs;

public class UpdateApplicationStatusCommand : IRequest<UpdateApplicationStatusResponseDto>
{
	public Guid ApplicationId { get; set; }
	public Guid RecruiterId { get; set; }
	public JobApplicationStatus Status { get; set; }
	public string? Notes { get; set; }
}