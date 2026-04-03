// RecruitAI.API/Controllers/v1/AIController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.AI;
using RecruitAI.Application.DTOs.Requests.AI;
using RecruitAI.Application.DTOs.Responses.AI;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.AI;
using RecruitAI.Application.Services;
using RecruitAI_API.Controllers.v1;

namespace RecruitAI.API.Controllers.v1
{
	[ApiController]
	[Route("api/v1/[controller]")]
	[Authorize]
	public class AIController : BaseController
	{
		private readonly IMatchingService _matchingService;

		public AIController(
			IMediator mediator,
			ILogger<AIController> logger,
			IMessageService messageService,
			IWorkContext workContext,
			IMatchingService matchingService)
			: base(mediator, logger, messageService, workContext)
		{
			_matchingService = matchingService;
		}

		/// <summary>
		/// Phân tích CV để trích xuất kỹ năng
		/// </summary>
		[HttpPost("analyze-cv")]
		[ProducesResponseType(typeof(AnalyzeCvResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(AnalyzeCvResponseDto), StatusCodes.Status202Accepted)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> AnalyzeCV([FromBody] AnalyzeCVCommand command)
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				return Unauthorized();

			command.UserId = userId.Value;
			var result = await _mediator.Send(command);

			if (result.Status == "processing")
				return Accepted(result);

			return Ok(result);
		}

		/// <summary>
		/// Lấy kết quả phân tích CV
		/// </summary>
		[HttpGet("analysis/{cvId}")]
		[ProducesResponseType(typeof(AnalysisResultDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AnalysisResultDto>> GetAnalysisResult(Guid cvId)
		{
			return await ExecuteAsync<AnalysisResultDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var query = new GetAnalysisResultQuery
				{
					CvId = cvId,
					UserId = userId.Value
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Match CV với Job (tính toán và lưu kết quả)
		/// </summary>
		[HttpPost("match-cv-job")]
		[ProducesResponseType(typeof(MatchCvJobResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<MatchCvJobResponseDto>> MatchCvJob([FromBody] MatchCvJobRequestDto request)
		{
			return await ExecuteAsync<MatchCvJobResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				return await _matchingService.CalculateAndSaveMatchAsync(
					request.CvId, request.JobId, userId.Value);
			});
		}


		/// <summary>
		/// Lấy kết quả match giữa CV và Job (đã lưu)
		/// </summary>
		[HttpGet("match")]
		[ProducesResponseType(typeof(MatchCvJobResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<MatchCvJobResponseDto>> GetMatch([FromQuery] Guid cvId, [FromQuery] Guid jobId)
		{
			return await ExecuteAsync<MatchCvJobResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				return await _matchingService.GetMatchResultAsync(cvId, jobId, userId.Value);
			});
		}

		/// <summary>
		/// Lấy tất cả kết quả match của một CV
		/// </summary>
		[HttpGet("match/cv/{cvId}")]
		[ProducesResponseType(typeof(CvMatchesListResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<CvMatchesListResponseDto>> GetMatchesByCvId(Guid cvId)
		{
			return await ExecuteAsync<CvMatchesListResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				return await _matchingService.GetAllMatchesByCvIdAsync(cvId, userId.Value);
			});
		}
	}
}