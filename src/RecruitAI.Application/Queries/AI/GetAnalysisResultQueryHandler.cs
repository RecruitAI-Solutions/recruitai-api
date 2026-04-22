// RecruitAI.Application/Queries/AI/GetAnalysisResultQueryHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses.AI;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Shared.DTOs;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.Application.Queries.AI
{
	public class GetAnalysisResultQueryHandler : IRequestHandler<GetAnalysisResultQuery, AnalysisResultDto>
	{
		private readonly IAnalysisService _analysisService;
		private readonly ILogger<GetAnalysisResultQueryHandler> _logger;

		public GetAnalysisResultQueryHandler(
			IAnalysisService analysisService,
			ILogger<GetAnalysisResultQueryHandler> logger)
		{
			_analysisService = analysisService;
			_logger = logger;
		}

		public async Task<AnalysisResultDto> Handle(GetAnalysisResultQuery request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Getting analysis result for CV {CvId}", request.CvId);

			return await _analysisService.GetAnalysisResultAsync(request.CvId, request.UserId);
		}
	}
}