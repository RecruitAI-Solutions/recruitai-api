using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Text.Json;
using RecruitAI.Application.Extensions;

namespace RecruitAI.Application.Commands.Admin
{
	public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, AdminUserStatusUpdateResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<UpdateUserStatusCommandHandler> _logger;
		private readonly IAuditLogService _auditLogService;
		private readonly IMediator _mediator;
		private readonly IMessageService _msg;

		public UpdateUserStatusCommandHandler(
			IUnitOfWork unitOfWork,
			ILogger<UpdateUserStatusCommandHandler> logger,
			IAuditLogService auditLogService,
			IMediator mediator,
			IMessageService msg)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_auditLogService = auditLogService;
			_mediator = mediator;
			_msg = msg;
		}

		public async Task<AdminUserStatusUpdateResponseDto> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Admin updating user {UserId} status to {Status}", request.UserId, request.Status);

			var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

			if (user == null)
				throw new BusinessException(ErrorCode.UserNotFound, "User not found");

			var oldStatus = user.Status;  

			user.Status = request.Status;
			user.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			var notification = new CreateNotificationCommand
			{
				UserId = user.Id,
				Title = _msg.Get("Notification.AccountStatusChanged.Title"),
				Content = string.Format(_msg.Get("Notification.AccountStatusChanged.Content"), request.Status.GetDisplayName(_msg)),
				Type = "account_update",
				Data = JsonSerializer.Serialize(new { NewStatus = request.Status.ToString(), Reason = request.Reason })
			};
			await _mediator.Send(notification, cancellationToken);

			await _auditLogService.LogAsync(
				AuditEntityType.User,
				AuditAction.ChangeStatus,
				user.Id.ToEntityId(),
				user.Email,
				oldStatus.ToString(),
				request.Status.ToString(),
				request.Reason,
				cancellationToken);

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("User {UserId} status updated to {Status}", request.UserId, request.Status);

			return new AdminUserStatusUpdateResponseDto
			{
				Id = user.Id,
				Status = user.Status,
				Reason = request.Reason,
				UpdatedAt = user.UpdatedAt.Value
			};
		}
	}
}