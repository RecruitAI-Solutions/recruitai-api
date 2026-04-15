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
using System.Text.Json;

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

	public ApplyJobCommandHandler(
		IUnitOfWork unitOfWork,
		IMatchingService matchingService,
		ILogger<ApplyJobCommandHandler> logger,
		IMessageService msg,
		IAuditLogService auditLogService,
		IAIRecommendationService aiRecommendationService,
		IConfiguration configuration,
		IMediator mediator)
	{
		_unitOfWork = unitOfWork;
		_matchingService = matchingService;
		_logger = logger;
		_msg = msg;
		_auditLogService = auditLogService;
		_aiRecommendationService = aiRecommendationService;
		_configuration = configuration;
		_mediator = mediator;
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

		// 3. Check if already applied
		var existingApplication = await _unitOfWork.JobApplications
			.GetByJobAndCvAsync(request.JobId, request.CvId, cancellationToken);

		if (existingApplication != null)
			_msg.Throw(ErrorCode.DuplicateEntry, "AlreadyApplied");

		// 4. Calculate or get match result
		var matchResult = await _matchingService.CalculateAndSaveMatchAsync(
			request.CvId, request.JobId, request.UserId, cancellationToken);

		// 5a. Create job application
		var application = new JobApplication
		{
			Id = Guid.NewGuid(),
			JobId = request.JobId,
			CVId = request.CvId,
			Status = JobApplicationStatus.Pending,
			AppliedAt = DateTime.UtcNow
		};

		await _unitOfWork.JobApplications.AddAsync(application, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// 5b. Tạo thông báo cho nhà tuyển dụng (recruiter)
		var recruiterNotification = new CreateNotificationCommand
		{
			UserId = job.RecruiterId,
			Title = _msg.Get("Notification.NewApplication.Title"),
			Content = string.Format(_msg.Get("Notification.NewApplication.Content"), job.Title, cv.FileName),
			Type = "application_update",
			Data = JsonSerializer.Serialize(new { ApplicationId = application.Id, JobId = job.Id, CvId = cv.Id })
		};

		await _mediator.Send(recruiterNotification, cancellationToken);

		// 5c. Tạo thông báo xác nhận cho ứng viên
		var candidateNotification = new CreateNotificationCommand
		{
			UserId = request.UserId,
			Title = _msg.Get("Notification.ApplicationSubmitted.Title"),
			Content = string.Format(_msg.Get("Notification.ApplicationSubmitted.Content"), job.Title),
			Type = "application_update",
			Data = JsonSerializer.Serialize(new { ApplicationId = application.Id, JobId = job.Id })
		};

		await _mediator.Send(candidateNotification, cancellationToken);

		// 6. Update match with application id
		var match = await _unitOfWork.JobApplicationMatches
			.GetByApplicationIdAsync(application.Id, cancellationToken);

		if (match == null)
		{
			match = new JobApplicationMatch
			{
				Id = Guid.NewGuid(),
				ApplicationId = application.Id,
				MatchPercentage = matchResult.MatchPercentage,
				RequiredSkillCount = matchResult.RequiredSkillCount,
				MatchedSkillCount = matchResult.MatchedSkillCount,
				MatchedSkillsJson = JsonSerializer.Serialize(matchResult.MatchedSkills),
				MissingSkillsJson = JsonSerializer.Serialize(matchResult.MissingSkills),
				CalculatedAt = DateTime.UtcNow
			};
			await _unitOfWork.JobApplicationMatches.AddAsync(match, cancellationToken);
		}
		else
		{
			match.MatchPercentage = matchResult.MatchPercentage;
			match.RequiredSkillCount = matchResult.RequiredSkillCount;
			match.MatchedSkillCount = matchResult.MatchedSkillCount;
			match.MatchedSkillsJson = JsonSerializer.Serialize(matchResult.MatchedSkills);
			match.MissingSkillsJson = JsonSerializer.Serialize(matchResult.MissingSkills);
			match.CalculatedAt = DateTime.UtcNow;
			_unitOfWork.JobApplicationMatches.Update(match);
		}

		// 7. Update job applications count
		job.Applications++;
		_unitOfWork.Jobs.Update(job);

		// 8. AI Recommendation (optional, không ảnh hưởng luồng chính)
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

		// 9. Ghi audit log
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

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(_msg.Log("UserAppliedForJob"),
			request.UserId, request.JobId, request.CvId);

		// 10. Trả về response
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
}