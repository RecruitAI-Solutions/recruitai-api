using RecruitAI.Application.DTOs;
using RecruitAI.Application.DTOs.Responses.AI;

namespace RecruitAI.Application.Services
{
	public interface IMatchingService
	{
		Task<MatchCvJobResponseDto> CalculateAndSaveMatchAsync(Guid cvId, Guid jobId, Guid currentUserId);
		Task<MatchCvJobResponseDto> GetMatchResultAsync(Guid cvId, Guid jobId, Guid currentUserId);
		Task<CvMatchesListResponseDto> GetAllMatchesByCvIdAsync(Guid cvId, Guid currentUserId);
	}
}