using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.DTOs.AI;
using RecruitAI.Application.DTOs.Responses.Applications;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Infrastructure.Services;
using System.Text.Json;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Commands.Applications;

public class ApplyJobCommandHandler : IRequestHandler<ApplyJobCommand, ApplyJobResponseDto>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMatchingService _matchingService;
	private readonly ILogger<ApplyJobCommandHandler> _logger;
	private readonly IMessageService _msg;
	private readonly IAuditLogService _auditLogService;
	private readonly IAIRecommendationService _aiRecommendationService;
	private readonly IConfiguration _configuration;
	private readonly IMediator _mediator;
	private readonly IEmailService _emailService;
	private readonly IWorkContext _workContext;
	private readonly IAppUrlService _appUrlService;

	public ApplyJobCommandHandler(
		IUnitOfWork unitOfWork,
		IMatchingService matchingService,
		ILogger<ApplyJobCommandHandler> logger,
		IMessageService msg,
		IAuditLogService auditLogService,
		IAIRecommendationService aiRecommendationService,
		IConfiguration configuration,
		IMediator mediator,
		IEmailService emailService,
		IWorkContext workContext,
		IAppUrlService appUrlService)
	{
		_unitOfWork = unitOfWork;
		_matchingService = matchingService;
		_logger = logger;
		_msg = msg;
		_emailService = emailService;
		_auditLogService = auditLogService;
		_aiRecommendationService = aiRecommendationService;
		_configuration = configuration;
		_mediator = mediator;
		_workContext = workContext;
		_appUrlService = appUrlService;
	}

	public async Task<ApplyJobResponseDto> Handle(ApplyJobCommand request, CancellationToken cancellationToken)
	{
		// 1. Validate Job
		var job = await _unitOfWork.Jobs.GetByIdAsync(request.JobId, cancellationToken);
		if (job == null)
			_msg.Throw(ErrorCode.ResourceNotFound, "JobNotFound");

		if (!job.IsActive || job.IsDeleted)
			_msg.Throw(ErrorCode.InvalidData, "JobNotActive");

		// 2. Validate CV
		var cv = await _unitOfWork.CVs.GetByIdAsync(request.CvId, cancellationToken);
		if (cv == null)
			_msg.Throw(ErrorCode.CVNotFound, "CVNotFound");

		if (cv.UserId != request.UserId)
			_msg.Throw(ErrorCode.Forbidden, "NoPermissionToUseCV");

		if (cv.Status != CVStatus.Analyzed)
			_msg.Throw(ErrorCode.InvalidData, "CVNotAnalyzed");

		// Lấy thông tin user từ CV
		var candidate = await _unitOfWork.Users.GetByIdAsync(cv.UserId, cancellationToken);
		if (candidate == null)
			_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");

		// 3. Check if already applied
		var existingApplication = await _unitOfWork.JobApplications
			.GetByJobAndCvAsync(request.JobId, request.CvId, cancellationToken);

		if (existingApplication != null)
			_msg.Throw(ErrorCode.DuplicateEntry, "AlreadyApplied");

		// 4. Begin transaction
		await _unitOfWork.BeginTransactionAsync(cancellationToken);

		try
		{
			// 5. Calculate or get match result (this also creates/updates application and match)
			var matchResult = await _matchingService.CalculateAndSaveMatchAsync(
				request.CvId, request.JobId, request.UserId, cancellationToken);

			// 6. Get the application that was created/updated by MatchingService
			var application = await _unitOfWork.JobApplications
				.GetByJobAndCvAsync(request.JobId, request.CvId, cancellationToken);

			if (application == null)
				throw new BusinessException(ErrorCode.InternalServerError, "Failed to create application");

			// 7. Update job applications count
			job.Applications++;
			_unitOfWork.Jobs.Update(job);

			// 8. Create notifications
			var recruiterNotification = new CreateNotificationCommand
			{
				UserId = job.RecruiterId,
				Title = _msg.Get("Notification.NewApplication.Title"),
				Content = string.Format(_msg.Get("Notification.NewApplication.Content"), job.Title, cv.FileName),
				Type = "application_update",
				Data = JsonSerializer.Serialize(new { ApplicationId = application.Id, JobId = job.Id, CvId = cv.Id })
			};
			await _mediator.Send(recruiterNotification, cancellationToken);

			var candidateNotification = new CreateNotificationCommand
			{
				UserId = request.UserId,
				Title = _msg.Get("Notification.ApplicationSubmitted.Title"),
				Content = string.Format(_msg.Get("Notification.ApplicationSubmitted.Content"), job.Title),
				Type = "application_update",
				Data = JsonSerializer.Serialize(new { ApplicationId = application.Id, JobId = job.Id })
			};
			await _mediator.Send(candidateNotification, cancellationToken);

			// 9. AI Recommendation (optional)
			AIRecommendationDto? aiAnalysis = null;
			var enableRecommendation = _configuration.GetValue<bool>("AI:EnableRecommendation", true);

			if (enableRecommendation)
			{
				try
				{
					var matchedSkillNames = matchResult.MatchedSkills.Select(s => s.Name).ToList();
					var missingSkillNames = matchResult.MissingSkills.Select(s => s.Name).ToList();
					var requiredSkillNames = job.JobSkills
						.Where(js => js.Skill != null)
						.Select(js => js.Skill!.Name)
						.ToList();

					var applicationCount = await _unitOfWork.JobApplications
						.CountByJobIdAsync(request.JobId, cancellationToken);

					aiAnalysis = await _aiRecommendationService.GetRecommendationAsync(
						matchedSkillNames,
						missingSkillNames,
						job.Title,
						requiredSkillNames,
						applicationCount,
						cancellationToken);

					_logger.LogInformation("AI recommendation generated for application {ApplicationId}", application.Id);
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "AI recommendation failed for application {ApplicationId}, continuing without analysis", application.Id);
				}
			}

			// 10. Build salary range display
			var salaryRange = _msg.GetSalaryRangeDisplay(job.SalaryMin, job.SalaryMax);

			// 11. Send confirmation email (non-blocking)
			try
			{
				var candidateName = candidate.FullName ?? "User";
				var candidateEmail = candidate.Email ?? string.Empty;

				if (!string.IsNullOrEmpty(candidateEmail))
				{
					await _emailService.SendJobApplicationEmailAsync(
						to: candidateEmail,
						userName: candidateName,
						jobTitle: job.Title,
						companyName: job.Company?.Name ?? "RecruitAI",
						jobLocation: job.Location ?? "Online",
						salaryRange: salaryRange,
						appliedDate: DateTime.UtcNow,
						trackingLink: $"{_appUrlService.GetClientUrl()}/applications/{application.Id}",
						cancellationToken
					);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to send application confirmation email for application {ApplicationId}", application.Id);
			}

			// 12. Audit log
			var applicationData = new Dictionary<string, string>
			{
				[_msg.Get("AuditFieldJobId")] = request.JobId.ToString(),
				[_msg.Get("AuditFieldCvId")] = request.CvId.ToString(),
				[_msg.Get("AuditFieldMatchPercentage")] = matchResult.MatchPercentage.ToString(),
				["hasAiAnalysis"] = (aiAnalysis != null).ToString()
			};

			await _auditLogService.LogAsync(
				AuditEntityType.Application,
				AuditAction.Apply,
				application.Id.ToEntityId(),
				$"{job.Title} - {cv.FileName}",
				null,
				JsonSerializer.Serialize(applicationData),
				null,
				cancellationToken);

			// 13. Commit transaction
			await _unitOfWork.CommitTransactionAsync(cancellationToken);

			_logger.LogInformation(_msg.Log("UserAppliedForJob"),
				request.UserId, request.JobId, request.CvId);

			// 14. Return response
			return new ApplyJobResponseDto
			{
				ApplicationId = application.Id,
				JobId = request.JobId,
				JobTitle = job.Title,
				CvId = request.CvId,
				CvName = cv.FileName,
				MatchPercentage = matchResult.MatchPercentage,
				MatchedSkillCount = matchResult.MatchedSkillCount,
				RequiredSkillCount = matchResult.RequiredSkillCount,
				MatchedSkills = matchResult.MatchedSkills,
				MissingSkills = matchResult.MissingSkills,
				Status = application.Status,
				StatusName = application.Status.ToString(),
				StatusDisplay = _msg.Get($"ApplicationStatus.{application.Status}"),
				AppliedAt = application.AppliedAt,
				AiAnalysis = aiAnalysis
			};
		}
		catch (Exception)
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			throw;
		}
	}
}