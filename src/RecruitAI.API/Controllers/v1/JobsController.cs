using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.Jobs;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.Jobs;
using RecruitAI.Domain.Interfaces;


namespace RecruitAI_API.Controllers.v1;
/// <summary>
/// Quản lý tin tuyển dụng
/// </summary>
[Authorize]
[Route("api/v1/[controller]")]
public class JobsController : BaseController
{
	public JobsController(
		IMediator mediator,
		ILogger<JobsController> logger,
		IMessageService messageService,
		IWorkContext workContext)
		: base(mediator, logger, messageService, workContext)
	{
	}

	/// <summary>
	/// Lấy danh sách jobs với phân trang và filter
	/// </summary>
	/// <param name="filter">Bộ lọc tìm kiếm</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Danh sách jobs phân trang</returns>
	[HttpGet]
	[AllowAnonymous]
	[ProducesResponseType(typeof(PaginationResponseDto<JobListDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<PaginationResponseDto<JobListDto>>> GetJobs(
		[FromQuery] JobFilterDto filter,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<PaginationResponseDto<JobListDto>>(async () =>
		{
			var query = new GetJobsQuery { Filter = filter };
			var result = await _mediator.Send(query, cancellationToken);
			return result;
		});
	}

	/// <summary>
	/// Lấy chi tiết job theo ID
	/// </summary>
	/// <param name="id">ID của job</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Chi tiết job</returns>
	[HttpGet("{id}")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<JobDetailDto>> GetJobById(
		Guid id,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<JobDetailDto>(async () =>
		{
			var query = new GetJobByIdQuery { Id = id };
			var result = await _mediator.Send(query, cancellationToken);

			// Tăng lượt xem
			if (result != null)
			{
				// command tăng view ở đây
				// await _mediator.Send(new IncrementJobViewsCommand { Id = id }, cancellationToken);
			}

			return result;
		});
	}

	/// <summary>
	/// Tạo job mới (Chỉ Recruiter)
	/// </summary>
	/// <param name="command">Thông tin job</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Job vừa tạo</returns>
	[HttpPost]
	[Authorize(Roles = "RECRUITER")] // Chỉ Recruiter mới được tạo
	[ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<JobDetailDto>> CreateJob(
		[FromBody] CreateJobCommand command,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<JobDetailDto>(async () =>
		{
			// Gán RecruiterId từ user hiện tại
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			command.RecruiterId = userId.Value;

			var result = await _mediator.Send(command, cancellationToken);
			return result;
		}, "JobCreated");
	}

	/// <summary>
	/// Cập nhật job (Chỉ Recruiter - chủ sở hữu)
	/// </summary>
	/// <param name="id">ID của job</param>
	/// <param name="command">Thông tin cập nhật</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Job sau khi cập nhật</returns>
	[HttpPut("{id}")]
	[Authorize(Roles = "RECRUITER")] // Chỉ Recruiter mới được sửa
	[ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<JobDetailDto>> UpdateJob(
		Guid id,
		[FromBody] UpdateJobCommand command,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<JobDetailDto>(async () =>
		{
			// Kiểm tra quyền sở hữu sẽ được thực hiện trong Handler
			command.Id = id;

			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			command.RecruiterId = userId.Value;

			var result = await _mediator.Send(command, cancellationToken);
			return result;
		}, "JobUpdated");
	}

	/// <summary>
	/// Xóa job (Chỉ Recruiter - chủ sở hữu)
	/// </summary>
	/// <param name="id">ID của job</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Kết quả xóa</returns>
	[HttpDelete("{id}")]
	[Authorize(Roles = "RECRUITER")] // Chỉ Recruiter mới được xóa
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> DeleteJob(
		Guid id,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var command = new DeleteJobCommand
			{
				Id = id,
				RecruiterId = userId.Value
			};

			await _mediator.Send(command, cancellationToken);
		});
	}

	/// <summary>
	/// Lấy danh sách jobs của Recruiter hiện tại
	/// </summary>
	/// <param name="filter">Bộ lọc tìm kiếm</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Danh sách jobs của recruiter</returns>
	[HttpGet("my-jobs")]
	[Authorize(Roles = "RECRUITER")]
	[ProducesResponseType(typeof(PaginationResponseDto<JobListDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<PaginationResponseDto<JobListDto>>> GetMyJobs(
		[FromQuery] JobFilterDto filter,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<PaginationResponseDto<JobListDto>>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var query = new GetRecruiterJobsQuery
			{
				RecruiterId = userId.Value,
				Filter = filter
			};
			var result = await _mediator.Send(query, cancellationToken);
			return result;
		});
	}

	/// <summary>
	/// Lấy danh sách jobs đã xóa (chỉ Admin)
	/// </summary>
	/// <param name="filter">Bộ lọc tìm kiếm</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Danh sách jobs đã xóa</returns>
	[HttpGet("deleted")]
	[Authorize(Roles = "ADMIN")]
	[ProducesResponseType(typeof(PaginationResponseDto<JobListDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<PaginationResponseDto<JobListDto>>> GetDeletedJobs(
		[FromQuery] JobFilterDto filter,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<PaginationResponseDto<JobListDto>>(async () =>
		{
			var query = new GetDeletedJobsQuery { Filter = filter };
			var result = await _mediator.Send(query, cancellationToken);
			return result;
		});
	}
}