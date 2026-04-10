using RecruitAI.Application.DTOs.AI;

namespace RecruitAI.Application.Interfaces.Services
{
	public interface IAIExtractionService
	{
		Task<List<ExtractedSkillDto>> ExtractSkillsAsync(
			string cvText,
			CancellationToken cancellationToken = default);
	}
}