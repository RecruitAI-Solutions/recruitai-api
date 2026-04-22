// RecruitAI.Application/Commands/AI/AnalyzeCVCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Shared.DTOs;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.Application.Commands.AI
{
	public class AnalyzeCVCommandHandler : IRequestHandler<AnalyzeCVCommand, AnalyzeCvResponseDto>
	{
		private readonly IAnalysisService _analysisService;
		private readonly ILogger<AnalyzeCVCommandHandler> _logger;

		public AnalyzeCVCommandHandler(
			IAnalysisService analysisService,
			ILogger<AnalyzeCVCommandHandler> logger)
		{
			_analysisService = analysisService;
			_logger = logger;
		}

		public async Task<AnalyzeCvResponseDto> Handle(AnalyzeCVCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Analyzing CV {CvId} for user {UserId}", request.CvId, request.UserId);

			var analyzeRequest = new AnalyzeCvRequestDto { CvId = request.CvId };
			var result = await _analysisService.AnalyzeCVAsync(analyzeRequest, request.UserId);

			return result;
		}
	}
}