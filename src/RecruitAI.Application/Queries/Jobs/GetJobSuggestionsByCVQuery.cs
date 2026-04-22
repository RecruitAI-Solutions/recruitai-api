// RecruitAI.Application/Queries/Jobs/GetJobSuggestionsByCVQuery.cs
using MediatR;
using RecruitAI.Shared.DTOs;

namespace RecruitAI.Application.Queries.Jobs
{
	public class GetJobSuggestionsByCVQuery : IRequest<List<JobMatchResultDto>>
	{
		public Guid CVId { get; set; }
		public int MinMatchSkills { get; set; } = 2;
	}
}