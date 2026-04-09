using MediatR;
using Microsoft.Extensions.Logging;
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

	public ApplyJobCommandHandler(
		IUnitOfWork unitOfWork,
		IMatchingService matchingService,
		ILogger<ApplyJobCommandHandler> logger,
		IMessageService msg,
		IAuditLogService auditLogService)  
	{
		_unitOfWork = unitOfWork;
		_matchingService = matchingService;
		_logger = logger;
		_msg = msg;
		_auditLogService = auditLogService;
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
			request.CvId, request.JobId, request.UserId);

		// 5. Create job application
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

		// Ghi audit log
		var applicationData = new Dictionary<string, string>
		{
			[_msg.Get("AuditFieldJobId")] = request.JobId.ToString(),
			[_msg.Get("AuditFieldCvId")] = request.CvId.ToString(),
			[_msg.Get("AuditFieldMatchPercentage")] = matchResult.MatchPercentage.ToString()
		};

		await _auditLogService.LogAsync(
			AuditEntityType.Application,
			AuditAction.Apply,
			application.Id,
			$"{job.Title} - {cv.FileName}",
			null,
			JsonSerializer.Serialize(applicationData),
			null,
			cancellationToken);

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(_msg.Log("UserAppliedForJob"),
			request.UserId, request.JobId, request.CvId);

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
			AppliedAt = application.AppliedAt
		};
	}
}