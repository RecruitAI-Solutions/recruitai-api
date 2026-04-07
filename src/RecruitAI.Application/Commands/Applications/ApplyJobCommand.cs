using MediatR;
using RecruitAI.Application.DTOs.Responses.Applications;

namespace RecruitAI.Application.Commands.Applications;

public class ApplyJobCommand : IRequest<ApplyJobResponseDto>
{
	public Guid JobId { get; set; }
	public Guid CvId { get; set; }
	public Guid UserId { get; set; }
}