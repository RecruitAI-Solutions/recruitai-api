using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Infrastructure.Services;
using RecruitAI.Application.Extensions;
using System.Text.Json;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.Application.Commands.Admin
{
	public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, AdminUserRoleUpdateResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<UpdateUserRoleCommandHandler> _logger;
		private readonly IRolePermissionService _rolePermissionService;
		private readonly IAuditLogService _auditLogService;
		private readonly IMessageService _msg;
		private readonly IMediator _mediator;

		public UpdateUserRoleCommandHandler(
			IUnitOfWork unitOfWork,
			ILogger<UpdateUserRoleCommandHandler> logger,
			IRolePermissionService rolePermissionService,
			IAuditLogService auditLogService,
			IMessageService msg,
			IMediator mediator)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_rolePermissionService = rolePermissionService;
			_auditLogService = auditLogService;
			_msg = msg;
			_mediator = mediator;
		}

		public async Task<AdminUserRoleUpdateResponseDto> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Admin updating user {UserId} role to {Role}", request.UserId, request.Role);

			var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

			if (user == null)
				throw new BusinessException(ErrorCode.UserNotFound, "User not found");

			if (user.Role == request.Role)
				throw new BusinessException(ErrorCode.InvalidData, "User already has this role");

			var oldRole = user.Role;
			user.Role = request.Role;
			user.UpdatedAt = DateTime.UtcNow;

			var roleCode = user.Role.ToString().ToUpper();
			var permissions = _rolePermissionService.GetPermissionsForRole(roleCode);
			user.SetPermissions(permissions);

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			await _auditLogService.LogAsync(
				AuditEntityType.User,
				AuditAction.ChangeRole,
				user.Id.ToEntityId(),
				user.Email,
				oldRole.ToString(),
				request.Role.ToString(),
				null,
				cancellationToken);

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("User {UserId} role changed from {OldRole} to {NewRole}",
				request.UserId, oldRole, user.Role);

			var notification = new CreateNotificationCommand
			{
				UserId = user.Id,
				Title = _msg.Get("Notification.RoleChanged.Title"),
				Content = string.Format(_msg.Get("Notification.RoleChanged.Content"), request.Role.GetDisplayName(_msg)),
				Type = "account_update",
				Data = JsonSerializer.Serialize(new { OldRole = oldRole.ToString(), NewRole = request.Role.ToString() })
			};
			await _mediator.Send(notification, cancellationToken);

			_logger.LogInformation("User {UserId} role changed from {OldRole} to {NewRole}",
				request.UserId, oldRole, user.Role);

			return new AdminUserRoleUpdateResponseDto
			{
				Id = user.Id,
				Role = user.Role,
				Permissions = user.GetPermissionList(),
				UpdatedAt = user.UpdatedAt.Value
			};
		}
	}
}