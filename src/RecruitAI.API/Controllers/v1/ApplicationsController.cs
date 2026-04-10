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

namespace RecruitAI.API.Controllers.v1
{
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
		/// Apply for a job
		/// </summary>
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
		/// Get my applications (for candidates)
		/// </summary>
		[HttpGet("me")]
		[Authorize(Policy = "ViewApplications")]
		[ProducesResponseType(typeof(PaginationResponseDto<MyApplicationDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<PaginationResponseDto<MyApplicationDto>>> GetMyApplications(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] JobApplicationStatus? status = null)
		{
			return await ExecuteAsync<PaginationResponseDto<MyApplicationDto>>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var query = new GetMyApplicationsQuery
				{
					UserId = userId.Value,
					Page = page,
					PageSize = pageSize,
					Status = status
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Get applications for a job (for recruiters)
		/// </summary>
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
		/// Update application status (for recruiters)
		/// </summary>
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
		/// Get application detail by id
		/// </summary>
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
	}
}