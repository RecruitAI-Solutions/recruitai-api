using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.Jobs;
using RecruitAI.Application.Commands.Applications;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Requests.Applications;
using RecruitAI.Application.DTOs.Requests.Applications;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.Jobs;
using RecruitAI.Application.Queries.Applications;
using RecruitAI.Domain.Enums;
using RecruitAI_API.Controllers.v1;
using RecruitAI.Shared.DTOs;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.API.Controllers.v1
{
	/// <summary>
	/// Quản lý đơn ứng tuyển
	/// </summary>
	[ApiController]
	[Route("api/v1/[controller]")]
	[Authorize]
	public class ApplicationsController : BaseController
	{
		public ApplicationsController(
			IMediator mediator,
			ILogger<ApplicationsController> logger,
			IMessageService messageService,
			IWorkContext workContext)
			: base(mediator, logger, messageService, workContext)
		{
		}

		/// <summary>
		/// Ứng tuyển công việc
		/// </summary>
		/// <param name="jobId">ID của công việc muốn ứng tuyển</param>
		/// <param name="request">ID của CV sử dụng để ứng tuyển</param>
		/// <returns>Kết quả ứng tuyển kèm phân tích AI</returns>
		[HttpPost("jobs/{jobId}/apply")]
		[Authorize(Policy = "ApplyJob")]
		[ProducesResponseType(typeof(ApplyJobResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<ActionResult<ApplyJobResponseDto>> ApplyJob(
			Guid jobId,
			[FromBody] ApplyJobRequestDto request)
		{
			return await ExecuteAsync<ApplyJobResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var command = new ApplyJobCommand
				{
					JobId = jobId,
					CvId = request.CvId,
					UserId = userId.Value
				};

				return await _mediator.Send(command);
			});
		}

		/// <summary>
		/// Lấy danh sách đơn ứng tuyển của tôi (ứng viên)
		/// </summary>
		/// <param name="page">Số trang</param>
		/// <param name="pageSize">Số lượng mỗi trang</param>
		/// <param name="status">Lọc theo trạng thái đơn</param>
		/// <returns>Danh sách đơn ứng tuyển của ứng viên hiện tại</returns>
		[HttpGet("me")]
		[Authorize(Policy = "ViewApplications")]
		[ProducesResponseType(typeof(PaginationResponseDto<MyApplicationDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<PaginationResponseDto<MyApplicationDto>>> GetMyApplications(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 10,
		[FromQuery] JobApplicationStatus? status = null,
		[FromQuery] DateTime? fromDate = null,
		[FromQuery] DateTime? toDate = null,
		[FromQuery] string? query = null,
		[FromQuery] string? sortBy = "appliedAt",
		[FromQuery] string? sortOrder = "desc")
		{
			return await ExecuteAsync<PaginationResponseDto<MyApplicationDto>>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var queryParams = new GetMyApplicationsQuery
				{
					UserId = userId.Value,
					Page = page,
					PageSize = pageSize,
					Status = status,
					FromDate = fromDate,
					ToDate = toDate,
					Query = query,
					SortBy = sortBy,
					SortOrder = sortOrder
				};

				return await _mediator.Send(queryParams);
			});
		}


		/// <summary>
		/// Lấy danh sách đơn ứng tuyển của một công việc (nhà tuyển dụng)
		/// </summary>
		/// <param name="jobId">ID của công việc</param>
		/// <param name="page">Số trang</param>
		/// <param name="pageSize">Số lượng mỗi trang</param>
		/// <param name="status">Lọc theo trạng thái</param>
		/// <param name="minMatch">Lọc theo điểm match tối thiểu</param>
		/// <param name="sortBy">Sắp xếp theo trường</param>
		/// <param name="sortOrder">Thứ tự sắp xếp</param>
		/// <returns>Danh sách ứng viên đã ứng tuyển</returns>
		[HttpGet("jobs/{jobId}/applications")]
		[Authorize(Policy = "ViewApplications")]
		[ProducesResponseType(typeof(PaginationResponseDto<JobApplicationDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<PaginationResponseDto<JobApplicationDto>>> GetJobApplications(
			Guid jobId,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] JobApplicationStatus? status = null,
			[FromQuery] int minMatch = 0,
			[FromQuery] string sortBy = "appliedAt",
			[FromQuery] string sortOrder = "desc")
		{
			return await ExecuteAsync<PaginationResponseDto<JobApplicationDto>>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var query = new GetJobApplicationsQuery
				{
					JobId = jobId,
					RecruiterId = userId.Value,
					Page = page,
					PageSize = pageSize,
					Status = status,
					MinMatch = minMatch,
					SortBy = sortBy,
					SortOrder = sortOrder
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Cập nhật trạng thái đơn ứng tuyển (nhà tuyển dụng)
		/// </summary>
		/// <param name="applicationId">ID của đơn ứng tuyển</param>
		/// <param name="request">Trạng thái mới và ghi chú</param>
		/// <returns>Kết quả cập nhật</returns>
		[HttpPatch("{applicationId}/status")]
		[Authorize(Policy = "UpdateApplicationStatus")]
		[ProducesResponseType(typeof(UpdateApplicationStatusResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<UpdateApplicationStatusResponseDto>> UpdateApplicationStatus(
			Guid applicationId,
			[FromBody] UpdateApplicationStatusRequestDto request)
		{
			return await ExecuteAsync<UpdateApplicationStatusResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var command = new UpdateApplicationStatusCommand
				{
					ApplicationId = applicationId,
					Status = request.Status,
					Notes = request.Notes,
					RecruiterId = userId.Value
				};

				return await _mediator.Send(command);
			});
		}

		/// <summary>
		/// Lấy chi tiết đơn ứng tuyển theo ID
		/// </summary>
		/// <param name="id">ID của đơn ứng tuyển</param>
		/// <returns>Chi tiết đơn ứng tuyển bao gồm thông tin CV và Job</returns>
		[HttpGet("{id}")]
		[Authorize(Policy = "ViewApplications")]
		[ProducesResponseType(typeof(ApplicationDetailResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApplicationDetailResponseDto>> GetApplicationDetail(Guid id)
		{
			return await ExecuteAsync<ApplicationDetailResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var query = new GetApplicationDetailQuery
				{
					ApplicationId = id,
					UserId = userId.Value
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Lấy danh sách đơn ứng tuyển của nhà tuyển dụng hiện tại (tất cả các job của recruiter)
		/// </summary>
		/// <param name="page">Số trang</param>
		/// <param name="pageSize">Số lượng mỗi trang</param>
		/// <param name="status">Lọc theo trạng thái đơn</param>
		/// <param name="jobId">Lọc theo công việc cụ thể</param>
		/// <param name="fromDate">Từ ngày</param>
		/// <param name="toDate">Đến ngày</param>
		/// <param name="query">Tìm theo tên ứng viên, email hoặc tên công việc</param>
		/// <param name="minMatch">Lọc theo match percentage tối thiểu</param>
		/// <param name="sortBy">Sắp xếp theo trường (appliedAt, matchPercentage, jobTitle, candidateName, status)</param>
		/// <param name="sortOrder">Thứ tự sắp xếp (asc/desc)</param>
		/// <returns>Danh sách đơn ứng tuyển của recruiter</returns>
		[HttpGet("recruiter/applications")]
		[Authorize(Roles = "RECRUITER")]
		[ProducesResponseType(typeof(PaginationResponseDto<RecruiterApplicationDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<PaginationResponseDto<RecruiterApplicationDto>>> GetRecruiterApplications(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] JobApplicationStatus? status = null,
			[FromQuery] Guid? jobId = null,
			[FromQuery] DateTime? fromDate = null,
			[FromQuery] DateTime? toDate = null,
			[FromQuery] string? query = null,
			[FromQuery] int? minMatch = null,
			[FromQuery] string? sortBy = "appliedAt",
			[FromQuery] string? sortOrder = "desc")
		{
			return await ExecuteAsync<PaginationResponseDto<RecruiterApplicationDto>>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var applicationsQuery = new GetRecruiterApplicationsQuery
				{
					RecruiterId = userId.Value,
					Page = page,
					PageSize = pageSize,
					Status = status,
					JobId = jobId,
					FromDate = fromDate,
					ToDate = toDate,
					Query = query,
					MinMatch = minMatch,
					SortBy = sortBy,
					SortOrder = sortOrder
				};

				return await _mediator.Send(applicationsQuery);
			});
		}
	}
}