using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.Admin;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Requests.Admin;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.Admin;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI_API.Controllers.v1;

namespace RecruitAI.API.Controllers.v1
{
	[ApiController]
	[Route("api/v1/admin")]
	[Authorize(Policy = "AdminOnly")]
	public class AdminController : BaseController
	{
		public AdminController(
			IMediator mediator,
			ILogger<AdminController> logger,
			IMessageService messageService,
			IWorkContext workContext)
			: base(mediator, logger, messageService, workContext)
		{
		}

		/// <summary>
		/// Get users list with pagination and filters
		/// </summary>
		[HttpGet("users")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserListResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<AdminUserListResponseDto>> GetUsers(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] string? role = null,
			[FromQuery] int? status = null,
			[FromQuery] string? keyword = null,
			[FromQuery] string sortBy = "createdAt",
			[FromQuery] string sortOrder = "desc")
		{
			return await ExecuteAsync<AdminUserListResponseDto>(async () =>
			{
				var query = new GetUsersListQuery
				{
					Page = page,
					PageSize = pageSize,
					Role = role,
					Status = status,
					Keyword = keyword,
					SortBy = sortBy,
					SortOrder = sortOrder
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Get user detail by id
		/// </summary>
		[HttpGet("users/{id}")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserDetailResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserDetailResponseDto>> GetUserDetail(Guid id)
		{
			return await ExecuteAsync<AdminUserDetailResponseDto>(async () =>
			{
				var query = new GetUserDetailQuery
				{
					UserId = id
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Update user information
		/// </summary>
		[HttpPut("users/{id}")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserDetailResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserDetailResponseDto>> UpdateUser(
			Guid id,
			[FromBody] AdminUserUpdateRequestDto request)
		{
			return await ExecuteAsync<AdminUserDetailResponseDto>(async () =>
			{
				var command = new UpdateUserCommand
				{
					UserId = id,
					FullName = request.FullName,
					PhoneNumber = request.PhoneNumber,
					Gender = request.Gender,
					DateOfBirth = request.DateOfBirth,
					Role = request.Role,
					Status = request.Status
				};

				return await _mediator.Send(command);
			});
		}

		/// <summary>
		/// Update user status (Active, Suspended, Banned)
		/// </summary>
		[HttpPatch("users/{id}/status")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserStatusUpdateResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserStatusUpdateResponseDto>> UpdateUserStatus(
			Guid id,
			[FromBody] AdminUserStatusUpdateRequestDto request)
		{
			return await ExecuteAsync<AdminUserStatusUpdateResponseDto>(async () =>
			{
				var command = new UpdateUserStatusCommand
				{
					UserId = id,
					Status = request.Status,
					Reason = request.Reason
				};

				return await _mediator.Send(command);
			});
		}

		/// <summary>
		/// Update user role (CANDIDATE, RECRUITER, ADMIN)
		/// </summary>
		[HttpPatch("users/{id}/role")]
		[Authorize(Policy = "ManageRoles")]
		[ProducesResponseType(typeof(AdminUserRoleUpdateResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserRoleUpdateResponseDto>> UpdateUserRole(
			Guid id,
			[FromBody] AdminUserRoleUpdateRequestDto request)
		{
			return await ExecuteAsync<AdminUserRoleUpdateResponseDto>(async () =>
			{
				var command = new UpdateUserRoleCommand
				{
					UserId = id,
					Role = request.Role
				};

				return await _mediator.Send(command);
			});
		}

		/// <summary>
		/// Delete user (soft delete)
		/// </summary>
		[HttpDelete("users/{id}")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserDeleteResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserDeleteResponseDto>> DeleteUser(Guid id)
		{
			return await ExecuteAsync<AdminUserDeleteResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var command = new DeleteUserCommand
				{
					UserId = id,
					CurrentAdminId = userId.Value
				};

				return await _mediator.Send(command);
			});
		}
		/// <summary>
		/// Get admin dashboard statistics
		/// </summary>
		[HttpGet("stats")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(StatsResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<StatsResponseDto>> GetStats(
		[FromQuery] DateTime? fromDate = null,
		[FromQuery] DateTime? toDate = null)
		{
			return await ExecuteAsync<StatsResponseDto>(async () =>
			{
				var query = new GetAdminStatsQuery
				{
					FromDate = fromDate,
					ToDate = toDate
				};
				return await _mediator.Send(query);
			});
		}
		/// <summary>
		/// Get audit logs list with pagination and filters
		/// </summary>
		[HttpGet("audit-logs")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(PaginationResponseDto<AuditLogResponseDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<PaginationResponseDto<AuditLogResponseDto>>> GetAuditLogs(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 20,
			[FromQuery] string? entityType = null,
			[FromQuery] string? action = null,
			[FromQuery] Guid? userId = null,
			[FromQuery] DateTime? fromDate = null,
			[FromQuery] DateTime? toDate = null,
			[FromQuery] string? keyword = null,
			[FromQuery] string sortBy = "changedAt",
			[FromQuery] string sortOrder = "desc")
		{
			return await ExecuteAsync<PaginationResponseDto<AuditLogResponseDto>>(async () =>
			{
				var query = new GetAuditLogsQuery
				{
					Page = page,
					PageSize = pageSize,
					EntityType = entityType,
					Action = action,
					UserId = userId,
					FromDate = fromDate,
					ToDate = toDate,
					Keyword = keyword,
					SortBy = sortBy,
					SortOrder = sortOrder
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Get audit log detail by id
		/// </summary>
		[HttpGet("audit-logs/{id}")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(AuditLogDetailResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AuditLogDetailResponseDto>> GetAuditLogDetail(Guid id)
		{
			return await ExecuteAsync<AuditLogDetailResponseDto>(async () =>
			{
				var query = new GetAuditLogDetailQuery { Id = id };
				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Get audit logs for a specific entity
		/// </summary>
		[HttpGet("audit-logs/entity/{entityType}/{entityId}")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(PaginationResponseDto<AuditLogResponseDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<PaginationResponseDto<AuditLogResponseDto>>> GetEntityAuditLogs(
			string entityType,
			Guid entityId,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 20)
		{
			return await ExecuteAsync<PaginationResponseDto<AuditLogResponseDto>>(async () =>
			{
				if (!Enum.TryParse<AuditEntityType>(entityType, true, out var parsedEntityType))
				{
					throw new BusinessException(ErrorCode.InvalidData, "Invalid entity type");
				}

				var query = new GetEntityAuditLogsQuery
				{
					EntityType = parsedEntityType,
					EntityId = entityId,
					Page = page,
					PageSize = pageSize
				};

				return await _mediator.Send(query);
			});
		}
	}
}