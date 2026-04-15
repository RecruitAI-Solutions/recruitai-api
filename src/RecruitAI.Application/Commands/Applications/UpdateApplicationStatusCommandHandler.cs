using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Application.Extensions;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Text.Json;

namespace RecruitAI.Application.Commands.Applications;

public class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, UpdateApplicationStatusResponseDto>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<UpdateApplicationStatusCommandHandler> _logger;
	private readonly IMessageService _msg;
	private readonly IAuditLogService _auditLogService;  
	private readonly IMediator _mediator;

	public UpdateApplicationStatusCommandHandler(
		IUnitOfWork unitOfWork,
		ILogger<UpdateApplicationStatusCommandHandler> logger,
		IMessageService msg,
		IAuditLogService auditLogService,
		IMediator mediator)  
	{
		_unitOfWork = unitOfWork;
		_logger = logger;
		_msg = msg;
		_auditLogService = auditLogService;
		_mediator = mediator;
	}

	public async Task<UpdateApplicationStatusResponseDto> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
	{
		var application = await _unitOfWork.JobApplications.GetByIdAsync(request.ApplicationId, cancellationToken);
		if (application == null)
			_msg.Throw(ErrorCode.ResourceNotFound, "ApplicationNotFound");

		var job = await _unitOfWork.Jobs.GetByIdAsync(application.JobId, cancellationToken);
		if (job == null || job.RecruiterId != request.RecruiterId)
			_msg.Throw(ErrorCode.Forbidden, "NoPermissionToUpdateApplication");

		var oldStatus = application.Status;  // Lưu giá trị cũ

		application.Status = request.Status;
		application.ReviewedAt = DateTime.UtcNow;

		if (!string.IsNullOrWhiteSpace(request.Notes))
			application.Notes = request.Notes;

		_unitOfWork.JobApplications.Update(application);

		// Ghi audit log
		await _auditLogService.LogAsync(
			AuditEntityType.Application,
			AuditAction.UpdateStatus,
			application.Id.ToEntityId(),
			$"{job.Title} - {application.CV?.FileName}",
			oldStatus.ToString(),
			request.Status.ToString(),
			request.Notes,
			cancellationToken);

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// Lấy thông tin CV (cần load thêm)
		var cv = await _unitOfWork.CVs.GetByIdAsync(application.CVId, cancellationToken);

		// Tạo thông báo cho ứng viên
		var notification = new CreateNotificationCommand
		{
			UserId = cv.UserId,  // Ứng viên là chủ sở hữu CV
			Title = _msg.Get("Notification.ApplicationStatusChanged.Title"),
			Content = string.Format(_msg.Get("Notification.ApplicationStatusChanged.Content"), job.Title, request.Status.GetDisplayName(_msg)),
			Type = "application_update",
			Data = JsonSerializer.Serialize(new { ApplicationId = application.Id, JobId = job.Id, NewStatus = request.Status.ToString() })
		};

		await _mediator.Send(notification, cancellationToken);

		_logger.LogInformation(_msg.Log("RecruiterUpdatedApplication"),
			request.RecruiterId, request.ApplicationId, request.Status);

		return new UpdateApplicationStatusResponseDto
		{
			ApplicationId = application.Id,
			Status = application.Status,
			Notes = application.Notes,
			UpdatedAt = application.ReviewedAt ?? DateTime.UtcNow
		};
	}
}