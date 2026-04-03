// RecruitAI.Application/Commands/AI/AnalyzeCVCommand.cs
using MediatR;
using RecruitAI.Application.DTOs.Responses.AI;

namespace RecruitAI.Application.Commands.AI
{
	public class AnalyzeCVCommand : IRequest<AnalyzeCvResponseDto>
	{
		public Guid CvId { get; set; }
		public Guid UserId { get; set; }
	}
}