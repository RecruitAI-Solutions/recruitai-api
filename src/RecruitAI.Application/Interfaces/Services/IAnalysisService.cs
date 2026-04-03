// RecruitAI.Application/Interfaces/Services/IAnalysisService.cs
using RecruitAI.Application.DTOs.Requests.AI;
using RecruitAI.Application.DTOs.Responses.AI;

namespace RecruitAI.Application.Interfaces.Services
{
	public interface IAnalysisService
	{
		Task<AnalyzeCvResponseDto> AnalyzeCVAsync(AnalyzeCvRequestDto request, Guid userId);
		Task<AnalysisResultDto> GetAnalysisResultAsync(Guid cvId, Guid userId);
	}
}