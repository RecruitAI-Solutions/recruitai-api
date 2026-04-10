using RecruitAI.Application.DTOs.AI;

namespace RecruitAI.Application.Interfaces.Services
{
	public interface IAIMatchingService
	{
		Task<AIMatchResponseDto> EvaluateMatchAsync(
			string cvText,
			string jobTitle,
			string jobDescription,
			string jobRequirements,
			decimal? salaryMin,
			decimal? salaryMax,
			string location,
			CancellationToken cancellationToken = default);
	}
}