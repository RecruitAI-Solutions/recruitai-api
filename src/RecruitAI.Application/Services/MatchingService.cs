using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.AI;
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
		private readonly IAIMatchingService _aiMatchingService;
		private readonly IConfiguration _configuration;
		private readonly ILogger<MatchingService> _logger;
		private readonly IMemoryCache _cache;

		private const string AI_MATCH_CACHE_KEY = "ai_match_";
		private const string MATCH_RESULT_CACHE_KEY = "match_result_";  

		public MatchingService(
			IUnitOfWork unitOfWork,
			IAuditLogService auditLogService,
			IAIMatchingService aiMatchingService,
			IConfiguration configuration,
			ILogger<MatchingService> logger,
			IMemoryCache cache)
		{
			_unitOfWork = unitOfWork;
			_auditLogService = auditLogService;
			_aiMatchingService = aiMatchingService;
			_configuration = configuration;
			_logger = logger;
			_cache = cache;
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
				throw new BusinessException(ErrorCode.InvalidData, "CV has not been analyzed yet. Please analyze CV first.");

			// 5. Get job required skills
			var requiredSkills = job.JobSkills.Where(js => js.IsRequired).ToList();
			if (!requiredSkills.Any())
				throw new BusinessException(ErrorCode.InvalidData, "Job has no required skills");

			// 6. Get current versions để kiểm tra thay đổi
			long cvVersion = cv.AnalyzedAt?.Ticks ?? cv.ProcessedAt?.Ticks ?? cv.UploadedAt.Ticks;
			long jobVersion = job.UpdatedAt?.Ticks ?? job.CreatedAt.Ticks;

			// 7. CHECK CACHE: Nếu không có thay đổi, trả về kết quả cũ
			string matchCacheKey = $"{MATCH_RESULT_CACHE_KEY}{cvId}_{jobId}";

			if (_cache.TryGetValue(matchCacheKey, out MatchCvJobResponseDto? cachedResult) && cachedResult != null)
			{
				// Kiểm tra version có thay đổi không
				if (cachedResult.CvVersion == cvVersion && cachedResult.JobVersion == jobVersion)
				{
					_logger.LogInformation("🎯 Using CACHED match result for CV {CvId} and Job {JobId} (Match: {Percentage}%, No changes detected)",
						cvId, jobId, cachedResult.MatchPercentage);
					return cachedResult;
				}
				else
				{
					_logger.LogInformation("🔄 Cache invalidated for CV {CvId} and Job {JobId} - Data has changed", cvId, jobId);
				}
			}

			// 8. Calculate basic match (skill-based)
			var cvSkillIds = cvSkills.Select(cs => cs.SkillId).ToHashSet();
			var requiredSkillIds = requiredSkills.Select(rs => rs.SkillId).ToHashSet();

			var matchedSkillIds = cvSkillIds.Intersect(requiredSkillIds).ToList();
			var missingSkillIds = requiredSkillIds.Except(cvSkillIds).ToList();

			var matchedCount = matchedSkillIds.Count;
			var requiredCount = requiredSkillIds.Count;
			var basicMatchPercentage = requiredCount > 0 ? (matchedCount * 100) / requiredCount : 0;

			// 9. Get skill details
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

			// 10. Try AI match for better accuracy
			int finalMatchPercentage = basicMatchPercentage;
			string? aiReason = null;
			bool usedAI = false;
			bool usedAICache = false;

			var enableAIMatch = _configuration.GetValue<bool>("AI:EnableAIMatch", true);
			var aiMinMatchToCall = _configuration.GetValue<int>("AI:MinMatchToCallAI", 20);
			var aiMaxMatchToCall = _configuration.GetValue<int>("AI:MaxMatchToCallAI", 80);
			var aiCacheMinutes = _configuration.GetValue<int>("AI:CacheMinutes", 60);

			// Kiểm tra Extract text
			bool hasExtractedText = !string.IsNullOrWhiteSpace(cv.ExtractedText);
			int extractedTextLength = cv.ExtractedText?.Length ?? 0;

			_logger.LogInformation("=== AI CHECK: EnableAI={Enable}, HasExtractedText={HasText}, ExtractedTextLength={Length}, BasicMatch={BasicMatch}%, CVVersion={Version}, JobVersion={JobVersion} ===",
				enableAIMatch, hasExtractedText, extractedTextLength, basicMatchPercentage, cvVersion, jobVersion);

			// CHỈ GỌI AI KHI:
			// 1. AI được bật
			// 2. CV có extracted text
			// 3. Basic match nằm trong khoảng cần đánh giá lại
			bool shouldCallAI = enableAIMatch
				&& hasExtractedText
				&& basicMatchPercentage >= aiMinMatchToCall
				&& basicMatchPercentage <= aiMaxMatchToCall;

			if (shouldCallAI)
			{
				try
				{
					// Tạo cache key dựa trên CV Id, Job Id, và version timestamp của CV
					string aiCacheKey = $"{AI_MATCH_CACHE_KEY}{cvId}_{jobId}_{cvVersion}";

					AIMatchResponseDto? aiResult = null;

					// Thử lấy từ cache
					if (_cache.TryGetValue(aiCacheKey, out AIMatchResponseDto? cachedAiResult) && cachedAiResult != null)
					{
						aiResult = cachedAiResult;
						usedAICache = true;
						_logger.LogInformation("Using cached AI result for CV {CvId} and Job {JobId}", cvId, jobId);
					}
					else
					{
						_logger.LogInformation("Calling AI match service for CV {CvId} and Job {JobId}", cvId, jobId);

						aiResult = await _aiMatchingService.EvaluateMatchAsync(
							cv.ExtractedText,
							job.Title,
							job.Description ?? "",
							job.Requirements ?? "",
							job.SalaryMin,
							job.SalaryMax,
							job.Location ?? "",
							cancellationToken);

						// Lưu vào cache
						if (aiResult != null)
						{
							var aiCacheOptions = new MemoryCacheEntryOptions
							{
								AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(aiCacheMinutes),
								Priority = CacheItemPriority.Normal
							};
							_cache.Set(aiCacheKey, aiResult, aiCacheOptions);
							_logger.LogInformation("Cached AI result for CV {CvId} and Job {JobId} (Expires in {Minutes} minutes)",
								cvId, jobId, aiCacheMinutes);
						}
					}

					if (aiResult != null)
					{
						finalMatchPercentage = aiResult.MatchPercentage;
						aiReason = aiResult.AiReason;
						usedAI = aiResult.UsedAI;

						_logger.LogInformation("AI match result: {Percentage}% for CV {CvId} (UsedAI={UsedAI}, FromCache={FromCache})",
							finalMatchPercentage, cvId, usedAI, usedAICache);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "AI match failed for CV {CvId}, using basic match result ({BasicPercentage}%)",
						cvId, basicMatchPercentage);
				}
			}
			else
			{
				string reason = !enableAIMatch ? "AI disabled in config" :
							   !hasExtractedText ? $"No extracted text (length={extractedTextLength})" :
							   $"Basic match {basicMatchPercentage}% outside range [{aiMinMatchToCall}-{aiMaxMatchToCall}]";

				_logger.LogWarning("AI skipped: {Reason}", reason);
			}

			// 11. Save to database
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
					MatchPercentage = finalMatchPercentage,
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
				existingMatch.MatchPercentage = finalMatchPercentage;
				existingMatch.RequiredSkillCount = requiredCount;
				existingMatch.MatchedSkillCount = matchedCount;
				existingMatch.MatchedSkillsJson = JsonSerializer.Serialize(matchedSkills);
				existingMatch.MissingSkillsJson = JsonSerializer.Serialize(missingSkills);
				existingMatch.CalculatedAt = DateTime.UtcNow;
				_unitOfWork.JobApplicationMatches.Update(existingMatch);
			}

			// 12. Ghi audit log
			var matchData = new Dictionary<string, string>
			{
				["cvId"] = cvId.ToString(),
				["jobId"] = jobId.ToString(),
				["basicMatchPercentage"] = basicMatchPercentage.ToString(),
				["finalMatchPercentage"] = finalMatchPercentage.ToString(),
				["usedAI"] = usedAI.ToString(),
				["usedAICache"] = usedAICache.ToString(),
				["matchedSkillsCount"] = matchedCount.ToString(),
				["requiredSkillsCount"] = requiredCount.ToString(),
				["extractedTextLength"] = extractedTextLength.ToString(),
				["cvVersion"] = cvVersion.ToString(),
				["jobVersion"] = jobVersion.ToString()
			};

			if (!string.IsNullOrEmpty(aiReason))
			{
				matchData["aiReason"] = aiReason;
			}

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

			// ⭐ 13. Lưu kết quả vào cache
			var response = new MatchCvJobResponseDto
			{
				CvId = cvId,
				JobId = jobId,
				MatchPercentage = finalMatchPercentage,
				RequiredSkillCount = requiredCount,
				MatchedSkillCount = matchedCount,
				MatchedSkills = matchedSkills,
				MissingSkills = missingSkills,
				CalculatedAt = DateTime.UtcNow,
				AiReason = aiReason,
				UsedAI = usedAI,
				CvVersion = cvVersion,      
				JobVersion = jobVersion    
			};

			var cacheOptions = new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60), // Cache 60 phút
				Priority = CacheItemPriority.Normal
			};
			_cache.Set(matchCacheKey, response, cacheOptions);
			_logger.LogInformation("📦 Cached match result for CV {CvId} and Job {JobId} (Match: {Percentage}%)",
				cvId, jobId, finalMatchPercentage);

			return response;
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
				CalculatedAt = match.CalculatedAt,
				AiReason = null,
				UsedAI = false
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