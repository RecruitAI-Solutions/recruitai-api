// RecruitAI.Application/Interfaces/Services/IAnalysisService.cs
using RecruitAI.Shared.DTOs;


namespace RecruitAI.Shared.Interfaces
{
	public interface IAnalysisService
	{
		Task<AnalyzeCvResponseDto> AnalyzeCVAsync(AnalyzeCvRequestDto request, Guid userId);
		Task<AnalysisResultDto> GetAnalysisResultAsync(Guid cvId, Guid userId);
	}
}