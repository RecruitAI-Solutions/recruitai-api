using MediatR;
using RecruitAI.Shared.DTOs;

namespace RecruitAI.Application.Queries.Candidate
{
	// GetCandidateDashboardQuery.cs
	public class GetCandidateDashboardQuery : IRequest<CandidateDashboardDto>
	{
		public Guid UserId { get; set; }
	}
}
