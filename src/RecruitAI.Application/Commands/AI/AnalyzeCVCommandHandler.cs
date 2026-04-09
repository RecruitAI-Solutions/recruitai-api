// RecruitAI.Application/Commands/AI/AnalyzeCVCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Requests.AI;
using RecruitAI.Application.DTOs.Responses.AI;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Infrastructure.Services;
using System.Text.Json;

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