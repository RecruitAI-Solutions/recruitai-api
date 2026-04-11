using RecruitAI.Application.DTOs.AI;

namespace RecruitAI.Application.Interfaces.Services;

public interface IAIRecommendationService
{
	Task<AIRecommendationDto> GetRecommendationAsync(
		List<string> matchedSkills,
		List<string> missingSkills,
		string jobTitle,
		List<string> requiredSkills,
		int applicationCount,
		CancellationToken cancellationToken = default);
}