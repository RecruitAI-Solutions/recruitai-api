using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.AI;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Text.Json;

namespace RecruitAI.Application.Services
{
	public class MatchingService : IMatchingService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAuditLogService _auditLogService;

		public MatchingService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
		{
			_unitOfWork = unitOfWork;
			_auditLogService = auditLogService;
		}

		public async Task<MatchCvJobResponseDto> CalculateAndSaveMatchAsync(
			Guid cvId,
			Guid jobId,
			Guid currentUserId,
			CancellationToken cancellationToken = default)
		{
			// 1. Validate CV
			var cv = await _unitOfWork.CVs.GetByIdAsync(cvId, cancellationToken);
			if (cv == null || cv.IsDeleted)
				throw new BusinessException(ErrorCode.CVNotFound, "CV not found");

			// 2. Validate Job
			var job = await _unitOfWork.Jobs.GetByIdAsync(jobId, cancellationToken);
			if (job == null || job.IsDeleted || !job.IsActive)
				throw new BusinessException(ErrorCode.JobNotFound, "Job not found or not active");

			// 3. Check permission
			var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId, cancellationToken);
			if (currentUser == null)
				throw new BusinessException(ErrorCode.UserNotFound, "User not found");

			bool isOwner = cv.UserId == currentUserId;
			bool isRecruiter = currentUser.Role == UserRole.RECRUITER;

			if (!isOwner && !isRecruiter)
				throw new BusinessException(ErrorCode.Forbidden, "You don't have permission to match this CV");

			// 4. Check CV has been analyzed
			var cvSkills = await _unitOfWork.CVAnalysisResults.GetByCvIdAsync(cvId);
			if (cvSkills == null || !cvSkills.Any())
				throw new BusinessException(ErrorCode.InvalidData, "CV has not been analyzed yet");

			// 5. Get job required skills
			var requiredSkills = job.JobSkills.Where(js => js.IsRequired).ToList();
			if (!requiredSkills.Any())
				throw new BusinessException(ErrorCode.InvalidData, "Job has no required skills");

			// 6. Calculate match
			var cvSkillIds = cvSkills.Select(cs => cs.SkillId).ToHashSet();
			var requiredSkillIds = requiredSkills.Select(rs => rs.SkillId).ToHashSet();

			var matchedSkillIds = cvSkillIds.Intersect(requiredSkillIds).ToList();
			var missingSkillIds = requiredSkillIds.Except(cvSkillIds).ToList();

			var matchedCount = matchedSkillIds.Count;
			var requiredCount = requiredSkillIds.Count;
			var matchPercentage = requiredCount > 0 ? (matchedCount * 100) / requiredCount : 0;

			// 7. Get skill details
			var allSkills = await _unitOfWork.Skills.GetAllAsync(cancellationToken);
			var skillDict = allSkills.ToDictionary(s => s.Id);

			var matchedSkills = matchedSkillIds.Select(id => new SkillMatchDetailDto
			{
				SkillId = id,
				Name = skillDict.ContainsKey(id) ? skillDict[id].Name : "Unknown",
				Category = skillDict.ContainsKey(id) ? skillDict[id].Category : null
			}).ToList();

			var missingSkills = missingSkillIds.Select(id => new SkillMatchDetailDto
			{
				SkillId = id,
				Name = skillDict.ContainsKey(id) ? skillDict[id].Name : "Unknown",
				Category = skillDict.ContainsKey(id) ? skillDict[id].Category : null
			}).ToList();

			// 8. Save to database
			var existingApplication = await _unitOfWork.JobApplications.GetByJobAndCvAsync(jobId, cvId, cancellationToken);

			if (existingApplication == null)
			{
				existingApplication = new JobApplication
				{
					Id = Guid.NewGuid(),
					CVId = cvId,
					JobId = jobId,
					Status = JobApplicationStatus.Pending,
					AppliedAt = DateTime.UtcNow
				};
				await _unitOfWork.JobApplications.AddAsync(existingApplication, cancellationToken);
			}

			var existingMatch = await _unitOfWork.JobApplicationMatches.GetByApplicationIdAsync(existingApplication.Id, cancellationToken);

			if (existingMatch == null)
			{
				existingMatch = new JobApplicationMatch
				{
					Id = Guid.NewGuid(),
					ApplicationId = existingApplication.Id,
					MatchPercentage = matchPercentage,
					RequiredSkillCount = requiredCount,
					MatchedSkillCount = matchedCount,
					MatchedSkillsJson = JsonSerializer.Serialize(matchedSkills),
					MissingSkillsJson = JsonSerializer.Serialize(missingSkills),
					CalculatedAt = DateTime.UtcNow
				};
				await _unitOfWork.JobApplicationMatches.AddAsync(existingMatch, cancellationToken);
			}
			else
			{
				existingMatch.MatchPercentage = matchPercentage;
				existingMatch.RequiredSkillCount = requiredCount;
				existingMatch.MatchedSkillCount = matchedCount;
				existingMatch.MatchedSkillsJson = JsonSerializer.Serialize(matchedSkills);
				existingMatch.MissingSkillsJson = JsonSerializer.Serialize(missingSkills);
				existingMatch.CalculatedAt = DateTime.UtcNow;
				_unitOfWork.JobApplicationMatches.Update(existingMatch);
			}

			var matchData = new Dictionary<string, string>
			{
				["cvId"] = cvId.ToString(),
				["jobId"] = jobId.ToString(),
				["matchPercentage"] = matchPercentage.ToString(),
				["matchedSkillsCount"] = matchedCount.ToString(),
				["requiredSkillsCount"] = requiredCount.ToString()
			};

			await _auditLogService.LogAsync(
				AuditEntityType.CV,
				AuditAction.Match,
				cvId.ToString(),
				$"{cv.FileName} - {job.Title}",
				null,
				JsonSerializer.Serialize(matchData),
				null,
				cancellationToken);


			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return new MatchCvJobResponseDto
			{
				CvId = cvId,
				JobId = jobId,
				MatchPercentage = matchPercentage,
				RequiredSkillCount = requiredCount,
				MatchedSkillCount = matchedCount,
				MatchedSkills = matchedSkills,
				MissingSkills = missingSkills,
				CalculatedAt = DateTime.UtcNow
			};
		}

		public async Task<MatchCvJobResponseDto> GetMatchResultAsync(
			Guid cvId,
			Guid jobId,
			Guid currentUserId,
			CancellationToken cancellationToken = default)
		{
			var cv = await _unitOfWork.CVs.GetByIdAsync(cvId, cancellationToken);
			if (cv == null)
				throw new BusinessException(ErrorCode.CVNotFound, "CV not found");

			var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId, cancellationToken);
			bool isOwner = cv.UserId == currentUserId;
			bool isRecruiter = currentUser?.Role == UserRole.RECRUITER;

			if (!isOwner && !isRecruiter)
				throw new BusinessException(ErrorCode.Forbidden, "You don't have permission");

			var application = await _unitOfWork.JobApplications.GetByJobAndCvAsync(jobId, cvId, cancellationToken);
			if (application == null)
				throw new BusinessException(ErrorCode.ResourceNotFound, "Match not found. Please calculate match first.");

			var match = await _unitOfWork.JobApplicationMatches.GetByApplicationIdAsync(application.Id, cancellationToken);
			if (match == null)
				throw new BusinessException(ErrorCode.ResourceNotFound, "Match result not found");

			var matchedSkills = JsonSerializer.Deserialize<List<SkillMatchDetailDto>>(match.MatchedSkillsJson) ?? new();
			var missingSkills = JsonSerializer.Deserialize<List<SkillMatchDetailDto>>(match.MissingSkillsJson) ?? new();

			return new MatchCvJobResponseDto
			{
				CvId = cvId,
				JobId = jobId,
				MatchPercentage = match.MatchPercentage,
				RequiredSkillCount = match.RequiredSkillCount,
				MatchedSkillCount = match.MatchedSkillCount,
				MatchedSkills = matchedSkills,
				MissingSkills = missingSkills,
				CalculatedAt = match.CalculatedAt
			};
		}

		public async Task<PaginationResponseDto<CvMatchSummaryDto>> GetAllMatchesByCvIdAsync(
		Guid cvId,
		Guid currentUserId,
		PaginationRequestDto pagination,
		int minMatch = 0,
		string sortBy = "matchPercentage",
		string sortOrder = "desc")
			{
				// Validate permission
				var cv = await _unitOfWork.CVs.GetByIdAsync(cvId);
				if (cv == null)
					throw new BusinessException(ErrorCode.CVNotFound, "CV not found");

				var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
				bool isOwner = cv.UserId == currentUserId;
				bool isRecruiter = currentUser?.Role == UserRole.RECRUITER;

				if (!isOwner && !isRecruiter)
					throw new BusinessException(ErrorCode.Forbidden, "You don't have permission");

				// Lấy tất cả applications của CV
				var applications = await _unitOfWork.JobApplications.GetByCvIdAsync(cvId);

				// Tạo danh sách match
				var allMatches = new List<CvMatchSummaryDto>();

				foreach (var app in applications)
				{
					var match = await _unitOfWork.JobApplicationMatches.GetByApplicationIdAsync(app.Id);
					if (match != null && match.MatchPercentage >= minMatch)
					{
						var job = await _unitOfWork.Jobs.GetByIdAsync(app.JobId);
						allMatches.Add(new CvMatchSummaryDto
						{
							JobId = app.JobId,
							JobTitle = job?.Title ?? "Unknown",
							Company = job?.Department ?? "Unknown",
							MatchPercentage = match.MatchPercentage,
							CalculatedAt = match.CalculatedAt
						});
					}
				}

				// Sorting
				allMatches = sortBy?.ToLower() switch
				{
					"jobtitle" => sortOrder == "asc"
						? allMatches.OrderBy(m => m.JobTitle).ToList()
						: allMatches.OrderByDescending(m => m.JobTitle).ToList(),
					"company" => sortOrder == "asc"
						? allMatches.OrderBy(m => m.Company).ToList()
						: allMatches.OrderByDescending(m => m.Company).ToList(),
					"calculatedat" => sortOrder == "asc"
						? allMatches.OrderBy(m => m.CalculatedAt).ToList()
						: allMatches.OrderByDescending(m => m.CalculatedAt).ToList(),
					_ => sortOrder == "asc"
						? allMatches.OrderBy(m => m.MatchPercentage).ToList()
						: allMatches.OrderByDescending(m => m.MatchPercentage).ToList()
				};

				// Pagination
				var total = allMatches.Count;
				var items = allMatches
					.Skip((pagination.Page - 1) * pagination.PageSize)
					.Take(pagination.PageSize)
					.ToList();

				return new PaginationResponseDto<CvMatchSummaryDto>
				{
					Data = items,
					Total = total,
					Page = pagination.Page,
					PageSize = pagination.PageSize
				};
			}
	}
}