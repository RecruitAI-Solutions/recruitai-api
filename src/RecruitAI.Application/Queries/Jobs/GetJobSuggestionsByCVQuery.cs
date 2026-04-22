// RecruitAI.Application/Queries/Jobs/GetJobSuggestionsByCVQuery.cs
using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Shared.DTOs;

namespace RecruitAI.Application.Queries.Jobs
{
	public class GetJobSuggestionsByCVQuery : IRequest<PaginationResponseDto<JobMatchResultDto>>
	{
		public Guid CVId { get; set; }
		public int MinMatchSkills { get; set; } = 2;
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}