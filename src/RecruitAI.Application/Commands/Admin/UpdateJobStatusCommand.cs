using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Shared.Interfaces;
using System.Text.Json;

namespace RecruitAI.Application.Commands.Admin
{
	public class UpdateJobStatusCommand : IRequest<AdminJobStatusUpdateResponseDto>
	{
		public Guid JobId { get; set; }
		public JobStatus Status { get; set; }
		public string? Reason { get; set; }
		public Guid AdminId { get; set; }
	}

	public class UpdateJobStatusCommandHandler : IRequestHandler<UpdateJobStatusCommand, AdminJobStatusUpdateResponseDto>
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<UpdateJobStatusCommandHandler> _logger;
		private readonly IAuditLogService _auditLogService;
		private readonly IMediator _mediator;
		private readonly IMessageService _msg;

		public UpdateJobStatusCommandHandler(
			IUnitOfWork uow,
			ILogger<UpdateJobStatusCommandHandler> logger,
			IAuditLogService auditLogService,
			IMediator mediator,
			IMessageService msg)
		{
			_uow = uow;
			_logger = logger;
			_auditLogService = auditLogService;
			_mediator = mediator;
			_msg = msg;
		}

		public async Task<AdminJobStatusUpdateResponseDto> Handle(UpdateJobStatusCommand request, CancellationToken cancellationToken)
		{
			var job = await _uow.Jobs.GetByIdAsync(request.JobId, cancellationToken);
			if (job == null)
				throw new BusinessException(ErrorCode.ResourceNotFound, _msg.Business("JobNotFound"));

			var oldStatus = job.Status;
			var oldStatusName = job.Status.ToString();

			job.Status = request.Status;
			job.UpdatedAt = DateTime.UtcNow;

			await _uow.SaveChangesAsync(cancellationToken);

			await _auditLogService.LogAsync(
				AuditEntityType.Job,
				AuditAction.UpdateJob,
				job.Id.ToString(),
				job.Title,
				oldStatusName,
				job.Status.ToString(),
				request.Reason ?? $"Status changed by admin",
				cancellationToken);

			var notification = new CreateNotificationCommand
			{
				UserId = job.RecruiterId,
				Title = _msg.Get("Notification.JobStatusChanged.Title"),
				Content = string.Format(_msg.Get("Notification.JobStatusChanged.Content"), job.Title, job.Status.ToString()),
				Type = "job_update",
				Data = JsonSerializer.Serialize(new { JobId = job.Id, JobTitle = job.Title, OldStatus = oldStatusName, NewStatus = job.Status.ToString() })
			};
			await _mediator.Send(notification, cancellationToken);

			return new AdminJobStatusUpdateResponseDto
			{
				JobId = job.Id,
				JobTitle = job.Title,
				OldStatus = oldStatus,
				OldStatusName = oldStatusName,
				NewStatus = job.Status,
				NewStatusName = job.Status.ToString(),
				IsActive = job.IsActive,
				UpdatedAt = job.UpdatedAt ?? DateTime.UtcNow,
				Success = true,
				Message = _msg.Success("JobStatusUpdated")
			};
		}
	}
}