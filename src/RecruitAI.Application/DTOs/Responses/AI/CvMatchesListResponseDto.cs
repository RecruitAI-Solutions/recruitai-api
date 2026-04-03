using RecruitAI.Application.DTOs.Common;

namespace RecruitAI.Application.DTOs.Responses.AI
{
	public class CvMatchesListResponseDto
	{
		public Guid CvId { get; set; }
		public List<CvMatchSummaryDto> Matches { get; set; } = new();
		public int TotalMatches => Matches.Count;
	}
}
