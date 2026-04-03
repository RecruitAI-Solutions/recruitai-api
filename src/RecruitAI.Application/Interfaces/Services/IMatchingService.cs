using RecruitAI.Application.DTOs;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.AI;

namespace RecruitAI.Application.Services
{
	public interface IMatchingService
	{
		Task<MatchCvJobResponseDto> CalculateAndSaveMatchAsync(
			Guid cvId,
			Guid jobId,
			Guid currentUserId,
			CancellationToken cancellationToken = default);

		Task<MatchCvJobResponseDto> GetMatchResultAsync(
			Guid cvId,
			Guid jobId,
			Guid currentUserId,
			CancellationToken cancellationToken = default);

		Task<PaginationResponseDto<CvMatchSummaryDto>> GetAllMatchesByCvIdAsync(
			Guid cvId,
			Guid currentUserId,
			PaginationRequestDto pagination,
			int minMatch = 0,
			string sortBy = "matchPercentage",
			string sortOrder = "desc");
	}
}