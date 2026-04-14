using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.Jobs;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Jobs;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.Jobs;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI_API.Controllers.v1;

namespace RecruitAI.API.Controllers.v1;

/// <summary>
/// Quản lý công việc đã lưu (chỉ CANDIDATE)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "CANDIDATE")]
public class SavedJobsController : BaseController
{
	public SavedJobsController(
		IMediator mediator,
		ILogger<SavedJobsController> logger,
		IMessageService messageService,
		IWorkContext workContext)
		: base(mediator, logger, messageService, workContext)
	{
	}

	/// <summary>
	/// Lưu công việc vào danh sách yêu thích
	/// </summary>
	/// <param name="jobId">ID công việc cần lưu</param>
	/// <returns>Kết quả lưu</returns>
	[HttpPost("jobs/{jobId}/save")]
	[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> SaveJob(Guid jobId)
	{
		return await ExecuteAsync(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var command = new SaveJobCommand { JobId = jobId, UserId = userId.Value };
			await _mediator.Send(command);
		}, _msg.Get("SaveJobSuccess"));
	}

	/// <summary>
	/// Lấy danh sách công việc đã lưu
	/// </summary>
	/// <param name="page">Số trang</param>
	/// <param name="pageSize">Số lượng mỗi trang</param>
	/// <returns>Danh sách công việc đã lưu</returns>
	[HttpGet("jobs/saved")]
	[ProducesResponseType(typeof(PaginationResponseDto<SavedJobResponseDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PaginationResponseDto<SavedJobResponseDto>>> GetSavedJobs(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 10)
	{
		return await ExecuteAsync<PaginationResponseDto<SavedJobResponseDto>>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var query = new GetSavedJobsQuery
			{
				UserId = userId.Value,
				Page = page,
				PageSize = pageSize
			};
			return await _mediator.Send(query);
		});
	}

	/// <summary>
	/// Bỏ lưu công việc
	/// </summary>
	/// <param name="jobId">ID công việc cần bỏ lưu</param>
	/// <returns>Kết quả bỏ lưu</returns>
	[HttpDelete("jobs/{jobId}/save")]
	[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UnsaveJob(Guid jobId)
	{
		return await ExecuteAsync(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var command = new UnsaveJobCommand { JobId = jobId, UserId = userId.Value };
			await _mediator.Send(command);

		}, _msg.Get("UnsaveJobSuccess"));
	}
}