using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.Users;
using RecruitAI.Application.DTOs.Responses.Users;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.Candidate;
using RecruitAI.Shared.DTOs;
using RecruitAI.Shared.Interfaces;
using RecruitAI_API.Controllers.v1;

namespace RecruitAI.API.Controllers.v1
{
	[ApiController]
	[Route("api/v1/users")]
	[Authorize]
	public class UsersController : BaseController
	{
		public UsersController(
			IMediator mediator,
			ILogger<UsersController> logger,
			IMessageService messageService,
			IWorkContext workContext)
			: base(mediator, logger, messageService, workContext)
		{
		}

		/// <summary>
		/// Upload avatar for current user
		/// </summary>
		[HttpPost("avatar")]
		[Authorize(Policy = "EditProfile")]
		[RequestSizeLimit(5 * 1024 * 1024)] // 5MB
		[ProducesResponseType(typeof(UploadAvatarResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<UploadAvatarResponseDto>> UploadAvatar(IFormFile avatar)
		{
			return await ExecuteAsync<UploadAvatarResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				if (avatar == null)
					throw new BadHttpRequestException("Avatar file is required");

				var command = new UploadAvatarCommand
				{
					UserId = userId.Value,
					Avatar = avatar
				};

				return await _mediator.Send(command);
			});
		}

		/// <summary>
		/// Delete avatar for current user
		/// </summary>
		[HttpDelete("avatar")]
		[Authorize(Policy = "EditProfile")]
		[ProducesResponseType(typeof(DeleteAvatarResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<DeleteAvatarResponseDto>> DeleteAvatar()
		{
			return await ExecuteAsync<DeleteAvatarResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var command = new DeleteAvatarCommand
				{
					UserId = userId.Value
				};

				return await _mediator.Send(command);
			});
		}

		[HttpGet("dashboard")]
		[Authorize(Roles = "CANDIDATE")]
		public async Task<ActionResult<CandidateDashboardDto>> GetDashboard()
		{
			return await ExecuteAsync<CandidateDashboardDto>(async () =>
			{
				var userId = GetCurrentUserId().Value;
				return await _mediator.Send(new GetCandidateDashboardQuery { UserId = userId });
			});
		}
	}
}