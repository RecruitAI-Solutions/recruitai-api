using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Application.Extensions;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
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
	private readonly IEmailService _emailService;
	private readonly IAppUrlService _appUrlService;
	private readonly IWorkContext _workContext;

	public UpdateApplicationStatusCommandHandler(
		IUnitOfWork unitOfWork,
		ILogger<UpdateApplicationStatusCommandHandler> logger,
		IMessageService msg,
		IAuditLogService auditLogService,
		IMediator mediator,
		IEmailService emailService,
		IAppUrlService appUrlService,
		IWorkContext workContext)
	{
		_unitOfWork = unitOfWork;
		_logger = logger;
		_msg = msg;
		_auditLogService = auditLogService;
		_mediator = mediator;
		_emailService = emailService;
		_appUrlService = appUrlService;
		_workContext = workContext;
	}

	public async Task<UpdateApplicationStatusResponseDto> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
	{
		// 1. Get application
		var application = await _unitOfWork.JobApplications.GetByIdAsync(request.ApplicationId, cancellationToken);
		if (application == null)
			_msg.Throw(ErrorCode.ResourceNotFound, "ApplicationNotFound");

		// 2. Get job and validate permission
		var job = await _unitOfWork.Jobs.GetByIdAsync(application.JobId, cancellationToken);
		if (job == null || job.RecruiterId != request.RecruiterId)
			_msg.Throw(ErrorCode.Forbidden, "NoPermissionToUpdateApplication");

		// 3. Get CV and candidate info
		var cv = await _unitOfWork.CVs.GetByIdAsync(application.CVId, cancellationToken);
		if (cv == null)
			_msg.Throw(ErrorCode.CVNotFound, "CVNotFound");

		var candidate = await _unitOfWork.Users.GetByIdAsync(cv.UserId, cancellationToken);
		if (candidate == null)
			_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");

		// 4. Save old status for audit
		var oldStatus = application.Status;

		// 5. Update status
		application.Status = request.Status;
		application.ReviewedAt = DateTime.UtcNow;

		if (!string.IsNullOrWhiteSpace(request.Notes))
			application.Notes = request.Notes;

		_unitOfWork.JobApplications.Update(application);

		// 6. Ghi audit log
		await _auditLogService.LogAsync(
			AuditEntityType.Application,
			AuditAction.UpdateStatus,
			application.Id.ToEntityId(),
			$"{job.Title} - {cv.FileName}",
			oldStatus.ToString(),
			request.Status.ToString(),
			request.Notes,
			cancellationToken);

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// 7. Create notification for candidate
		var statusDisplayName = GetStatusDisplayName(request.Status, _msg);
		var notification = new CreateNotificationCommand
		{
			UserId = cv.UserId,
			Title = _msg.Get("Notification.ApplicationStatusChanged.Title"),
			Content = string.Format(_msg.Get("Notification.ApplicationStatusChanged.Content"), job.Title, statusDisplayName),
			Type = "application_update",
			Data = JsonSerializer.Serialize(new { ApplicationId = application.Id, JobId = job.Id, NewStatus = request.Status.ToString() })
		};
		await _mediator.Send(notification, cancellationToken);

		// 8. Send email to candidate based on status (non-blocking)
		await SendStatusUpdateEmailAsync(candidate, job, application, request.Status, statusDisplayName, cancellationToken);

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

	private string GetStatusDisplayName(JobApplicationStatus status, IMessageService msg)
	{
		return status switch
		{
			JobApplicationStatus.Pending => msg.Get("ApplicationStatus.Pending"),
			JobApplicationStatus.Reviewed => msg.Get("ApplicationStatus.Reviewed"),
			JobApplicationStatus.Accepted => msg.Get("ApplicationStatus.Accepted"),
			JobApplicationStatus.Rejected => msg.Get("ApplicationStatus.Rejected"),
			_ => status.ToString()
		};
	}

	private async Task SendStatusUpdateEmailAsync(
		User candidate,
		Job job,
		JobApplication application,
		JobApplicationStatus newStatus,
		string statusDisplayName,
		CancellationToken cancellationToken)
	{
		try
		{
			var candidateEmail = candidate.Email;
			if (string.IsNullOrEmpty(candidateEmail))
			{
				_logger.LogWarning("Cannot send status update email: candidate email is empty");
				return;
			}

			var clientUrl = _appUrlService.GetClientUrl();
			var trackingLink = $"{clientUrl}/applications/{application.Id}";
			var companyName = job.Company?.Name ?? "RecruitAI";

			switch (newStatus)
			{
				case JobApplicationStatus.Reviewed:
					await _emailService.SendApplicationReviewedEmailAsync(
						to: candidateEmail,
						userName: candidate.FullName,
						jobTitle: job.Title,
						companyName: companyName,
						status: statusDisplayName,
						notes: application.Notes,
						trackingLink: trackingLink,
						cancellationToken
					);
					break;

				case JobApplicationStatus.Accepted:
					await _emailService.SendApplicationAcceptedEmailAsync(
						to: candidateEmail,
						userName: candidate.FullName,
						jobTitle: job.Title,
						companyName: companyName,
						status: statusDisplayName,
						nextSteps: application.Notes ?? _msg.Get("DefaultAcceptedNextSteps"),
						trackingLink: trackingLink,
						cancellationToken
					);
					break;

				case JobApplicationStatus.Rejected:
					await _emailService.SendApplicationRejectedEmailAsync(
						to: candidateEmail,
						userName: candidate.FullName,
						jobTitle: job.Title,
						companyName: companyName,
						reason: application.Notes ?? _msg.Get("DefaultRejectionReason"),
						trackingLink: trackingLink,
						cancellationToken
					);
					break;

				case JobApplicationStatus.Pending:
				default:
					await _emailService.SendApplicationStatusUpdateEmailAsync(
						to: candidateEmail,
						userName: candidate.FullName,
						jobTitle: job.Title,
						companyName: companyName,
						status: statusDisplayName,
						notes: application.Notes,
						trackingLink: trackingLink,
						cancellationToken
					);
					break;
			}

			_logger.LogInformation("Status update email sent to {Email} for application {ApplicationId}",
				candidateEmail, application.Id);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to send status update email for application {ApplicationId}", application.Id);
		}
	}
}