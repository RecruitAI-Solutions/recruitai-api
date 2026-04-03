// RecruitAI.Application/Queries/AI/GetAnalysisResultQuery.cs
using MediatR;
using RecruitAI.Application.DTOs.Responses.AI;

namespace RecruitAI.Application.Queries.AI
{
	public class GetAnalysisResultQuery : IRequest<AnalysisResultDto>
	{
		public Guid CvId { get; set; }
		public Guid UserId { get; set; }
	}
}