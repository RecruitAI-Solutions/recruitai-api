// RecruitAI.API/Controllers/v1/AIController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.AI;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Requests.AI;
using RecruitAI.Domain.Enums;
using RecruitAI.Application.DTOs.Responses.AI;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.AI;
using RecruitAI.Application.Services;
using RecruitAI_API.Controllers.v1;
using RecruitAI.Shared.DTOs;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.API.Controllers.v1
{
	/// <summary>
	/// AI Controller - Xử lý phân tích CV và matching với công việc
	/// </summary>
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
		/// <remarks>
		/// Hệ thống sẽ đọc nội dung CV, sử dụng AI để nhận diện các kỹ năng 
		/// và lưu vào cơ sở dữ liệu.
		/// </remarks>
		/// <returns>Kết quả phân tích (Processing hoặc Completed)</returns>
		[Authorize(Policy = "AnalyzeCV")]
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

			if (result.Status == (int)CVStatus.Processing)
				return Accepted(result);

			return Ok(result);
		}

		/// <summary>
		/// Lấy kết quả phân tích CV
		/// </summary>
		/// <param name="cvId">ID của CV đã phân tích</param>
		/// <returns>Danh sách kỹ năng đã trích xuất và độ tin cậy</returns>
		[Authorize(Policy = "ViewCVAnalysis")]
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
		/// Match CV với công việc (tính toán và lưu kết quả)
		/// </summary>
		/// <remarks>
		/// Sử dụng AI để đánh giá độ phù hợp dựa trên:
		/// - Kỹ năng (50%)
		/// - Kinh nghiệm (25%)
		/// - Mức lương (15%)
		/// - Địa điểm (10%)
		/// </remarks>
		/// <returns>Điểm match chi tiết và phân tích từ AI</returns>
		[Authorize(Policy = "MatchCVJob")]
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
		/// Lấy kết quả match giữa CV và công việc (đã lưu)
		/// </summary>
		/// <param name="cvId">ID của CV</param>
		/// <param name="jobId">ID của công việc</param>
		/// <returns>Kết quả match đã lưu trước đó</returns>
		[Authorize(Policy = "ViewMatchResults")]
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
		/// <param name="cvId">ID của CV</param>
		/// <param name="page">Số trang</param>
		/// <param name="pageSize">Số lượng mỗi trang</param>
		/// <param name="minMatch">Lọc theo điểm match tối thiểu</param>
		/// <param name="sortBy">Sắp xếp theo trường</param>
		/// <param name="sortOrder">Thứ tự sắp xếp</param>
		/// <returns>Danh sách các công việc đã match với điểm số</returns>
		[Authorize(Policy = "ViewMatchResults")]
		[HttpGet("match/cv/{cvId}")]
		[ProducesResponseType(typeof(PaginationResponseDto<CvMatchSummaryDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<PaginationResponseDto<CvMatchSummaryDto>>> GetMatchesByCvId(
		Guid cvId,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 10,
		[FromQuery] int minMatch = 0,
		[FromQuery] string sortBy = "matchPercentage",
		[FromQuery] string sortOrder = "desc")
		{
			return await ExecuteAsync<PaginationResponseDto<CvMatchSummaryDto>>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var request = new PaginationRequestDto
				{
					Page = page,
					PageSize = pageSize
				};

				return await _matchingService.GetAllMatchesByCvIdAsync(
					cvId, userId.Value, request, minMatch, sortBy, sortOrder);
			});
		}
	}
}