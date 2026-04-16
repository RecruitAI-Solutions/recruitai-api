using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Text.Json;

namespace RecruitAI.Application.Commands.Jobs;

public class DeleteJobCommand : IRequest
{
	public Guid Id { get; set; }
	public Guid RecruiterId { get; set; }
}

public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand>
{
	private readonly IUnitOfWork _uow;
	private readonly ILogger<DeleteJobCommandHandler> _logger;
	private readonly IAuditLogService _auditLogService;
	private readonly IMediator _mediator;  
	private readonly IMessageService _msg;

	public DeleteJobCommandHandler(
		IUnitOfWork uow,
		ILogger<DeleteJobCommandHandler> logger,
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

	public async Task Handle(DeleteJobCommand request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Deleting job {JobId} by recruiter {RecruiterId}",
				request.Id, request.RecruiterId);

			// Kiểm tra job tồn tại
			var job = await _uow.Jobs.GetByIdAsync(request.Id, cancellationToken);
			if (job == null)
			{
				throw new BusinessException(
					ErrorCode.ResourceNotFound,
					"Không tìm thấy công việc");
			}

			// Kiểm tra quyền sở hữu
			if (job.RecruiterId != request.RecruiterId)
			{
				throw new BusinessException(
					ErrorCode.Forbidden,
					"Bạn không có quyền xóa công việc này");
			}

			// Ghi audit log trước khi xóa
			await _auditLogService.LogAsync(
				AuditEntityType.Job,
				AuditAction.DeleteJob,
				job.Id.ToEntityId(),
				job.Title,
				null,
				null,
				null,
				cancellationToken);

			// Soft delete
			job.MarkAsDeleted();
			await _uow.SaveChangesAsync(cancellationToken);

			var notification = new CreateNotificationCommand
			{
				UserId = request.RecruiterId,
				Title = _msg.Get("Notification.JobDeleted.Title"),
				Content = string.Format(_msg.Get("Notification.JobDeleted.Content"), job.Title),
				Type = "job_update",
				Data = JsonSerializer.Serialize(new { JobId = job.Id, JobTitle = job.Title })
			};
			await _mediator.Send(notification, cancellationToken);

			_logger.LogInformation("Notification sent to recruiter {RecruiterId} for job deletion", request.RecruiterId);

			_logger.LogInformation("Job {JobId} deleted successfully", request.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting job {JobId}", request.Id);
			throw;
		}
	}
}