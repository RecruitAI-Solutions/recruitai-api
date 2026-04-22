using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.DTOs.Requests.AI;
using RecruitAI.Application.DTOs.Responses.AI;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Domain.Interfaces.Services;
using System.Text.Json;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.DTOs;									

namespace RecruitAI.Application.Services
{
	public class AnalysisService : IAnalysisService
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<AnalysisService> _logger;
		private readonly IMessageService _msg;
		private readonly IPdfService _pdfService;
		private readonly IAuditLogService _auditLogService;
		private readonly IAIExtractionService _aiExtractionService;
		private readonly IConfiguration _configuration;
		private readonly IMediator _mediator;

		public AnalysisService(
			IUnitOfWork uow,
			ILogger<AnalysisService> logger,
			IMessageService msg,
			IPdfService pdfService,
			IAuditLogService auditLogService,
			IAIExtractionService aiExtractionService,
			IConfiguration configuration,
			IMediator mediator)
		{
			_uow = uow;
			_logger = logger;
			_msg = msg;
			_pdfService = pdfService;
			_auditLogService = auditLogService;
			_aiExtractionService = aiExtractionService;
			_configuration = configuration;
			_mediator = mediator;
		}

		public async Task<AnalyzeCvResponseDto> AnalyzeCVAsync(AnalyzeCvRequestDto request, Guid userId)
		{
			var cv = await _uow.CVs.GetByIdAsync(request.CvId);
			if (cv == null)
				throw new BusinessException(ErrorCode.CVNotFound, _msg.Business("CVNotFound"));

			if (cv.UserId != userId)
				throw new BusinessException(ErrorCode.Forbidden, _msg.Business("AccessDenied"));

			// Lấy text từ CV
			var cvText = await GetCVText(cv);

			// 1. Lấy kết quả AI (từ cache hoặc gọi API)
			var aiSkills = await GetAISkills(cv, cvText);
			bool usedCache = aiSkills.UsedCache;
			bool aiAvailable = aiSkills.Skills.Any();

			// 2. Luôn chạy rule-based (cập nhật mới nhất)
			var ruleBasedSkills = await RunRuleBasedExtraction(cvText);

			// 3. Kết hợp kết quả (ưu tiên AI, bổ sung từ rule-based)
			var combinedSkills = CombineSkills(aiSkills.Skills, ruleBasedSkills);

			// 4. Lưu kết quả vào database (chỉ lưu skill có trong DB)
			var analysisResults = combinedSkills
				.Where(s => s.SkillId > 0)
				.Select(s => new CVAnalysisResult
				{
					CVId = cv.Id,
					SkillId = s.SkillId,
					Confidence = s.Confidence,
					CreatedAt = DateTime.UtcNow
				});

			await _uow.CVAnalysisResults.RemoveByCVIdAsync(cv.Id);
			if (analysisResults.Any())
			{
				await _uow.CVAnalysisResults.AddRangeAsync(analysisResults);
			}

			// 5. Cập nhật trạng thái CV
			cv.Status = CVStatus.Analyzed;
			cv.AnalyzedAt = DateTime.UtcNow;
			await _uow.CVs.UpdateAsync(cv);
			await _uow.SaveChangesAsync();

			// 6. Ghi audit log
			await _auditLogService.LogAsync(
				AuditEntityType.CV,
				AuditAction.Analyze,
				cv.Id.ToEntityId(),
				cv.FileName,
				null,
				JsonSerializer.Serialize(new
				{
					TotalSkills = combinedSkills.Count,
					AISkillsCount = aiSkills.Skills.Count,
					RuleBasedSkillsCount = ruleBasedSkills.Count,
					UsedAICache = usedCache
				}),
				null,
				default);

			var notification = new CreateNotificationCommand
			{
				UserId = cv.UserId,
				Title = _msg.Get("Notification.CVAnalyzed.Title"),
				Content = string.Format(_msg.Get("Notification.CVAnalyzed.Content"), cv.FileName, combinedSkills.Count),
				Type = "cv_processed",
				Data = JsonSerializer.Serialize(new { CvId = cv.Id, TotalSkills = combinedSkills.Count })
			};
			await _mediator.Send(notification, default);

			// 7. Trả về kết quả
			return new AnalyzeCvResponseDto
			{
				CvId = cv.Id,
			  Status = (int)cv.Status,
				StatusName = cv.Status.ToString(),
				Skills = combinedSkills.OrderByDescending(s => s.Confidence).ToList(),
				TotalSkills = combinedSkills.Count,
				ProcessedAt = DateTime.UtcNow,
				AIAnalysis = new AIAnalysisInfoDto
				{
					IsAvailable = aiAvailable,
					UsedCache = usedCache,
					Skills = aiSkills.Skills.OrderByDescending(s => s.Confidence).ToList(),
					TotalSkills = aiSkills.Skills.Count
				}
			};
		}

		private string MapStatus(Domain.Enums.CVStatus status)
		{
			switch (status)
			{
				case CVStatus.Pending:
					return "pending";
				case CVStatus.Uploaded:
					return "pending"; // treat uploaded as pending analysis
				case CVStatus.Processing:
					return "processing";
				case CVStatus.Completed:
					return "completed";
				case CVStatus.Analyzed:
					return "analyzed";
				case CVStatus.Failed:
					return "failed";
				default:
					return status.ToString().ToLower();
			}
		}

		private async Task<string> GetCVText(CV cv)
		{
			if (!string.IsNullOrWhiteSpace(cv.ExtractedText))
				return cv.ExtractedText;

			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cv.FilePath);
			if (File.Exists(filePath))
			{
				_logger.LogInformation("Extracting text from PDF file: {FilePath}", filePath);
				cv.ExtractedText = await _pdfService.ExtractTextAsync(filePath);
				cv.Status = CVStatus.Completed;
				await _uow.SaveChangesAsync();
				return cv.ExtractedText;
			}

			throw new BusinessException(ErrorCode.InvalidData, "CV has no extracted text and PDF file not found");
		}

		private async Task<(List<SkillMatchDto> Skills, bool UsedCache)> GetAISkills(CV cv, string cvText)
		{
			// Kiểm tra cache trong database
			var cachedResults = await _uow.CVAnalysisResults.GetByCvIdAsync(cv.Id);

			if (cachedResults != null && cachedResults.Any())
			{
				_logger.LogInformation("Using cached AI results for CV {CvId}", cv.Id);
				var allSkills = await _uow.Skills.GetAllAsync();
				var skillDict = allSkills.ToDictionary(s => s.Id);

				var skills = cachedResults
					.Where(r => skillDict.ContainsKey(r.SkillId))
					.Select(r => new SkillMatchDto
					{
						SkillId = r.SkillId,
						Name = skillDict[r.SkillId].Name,
						Category = skillDict[r.SkillId].Category,
						Confidence = r.Confidence
					}).ToList();

				return (skills, true);
			}

			// Gọi AI service
			try
			{
				_logger.LogInformation("Calling AI service for CV {CvId}", cv.Id);
				var extractedSkills = await _aiExtractionService.ExtractSkillsAsync(cvText);

				var skills = new List<SkillMatchDto>();
				foreach (var extracted in extractedSkills)
				{
					var existingSkill = await _uow.Skills.GetByNameAsync(extracted.Name);
					int skillId;

					if (existingSkill != null)
					{
						skillId = existingSkill.Id;
					}
					else
					{
						// Tạo skill mới nếu chưa có
						var newSkill = new Skill
						{
							Name = extracted.Name,
							Category = extracted.Category ?? "Other",
							IsActive = true,
							CreatedAt = DateTime.UtcNow,
							CreatedBy = "AI"
						};
						await _uow.Skills.AddAsync(newSkill);
						await _uow.SaveChangesAsync();
						skillId = newSkill.Id;
					}

					skills.Add(new SkillMatchDto
					{
						SkillId = skillId,
						Name = extracted.Name,
						Category = extracted.Category,
						Confidence = extracted.Confidence
					});
				}

				return (skills, false);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "AI extraction failed for CV {CvId}", cv.Id);
				return (new List<SkillMatchDto>(), false);
			}
		}

		private async Task<List<SkillMatchDto>> RunRuleBasedExtraction(string cvText)
		{
			var allSkills = await _uow.Skills.GetAllActiveAsync();
			var matchedSkills = new List<SkillMatchDto>();
			var cvTextLower = cvText.ToLower();

			foreach (var skill in allSkills)
			{
				var confidence = CalculateConfidence(cvTextLower, skill);
				if (confidence > 0.3)
				{
					matchedSkills.Add(new SkillMatchDto
					{
						SkillId = skill.Id,
						Name = skill.Name,
						Category = skill.Category,
						Confidence = confidence
					});
				}
			}

			return matchedSkills;
		}

		private List<SkillMatchDto> CombineSkills(
			List<SkillMatchDto> aiSkills,
			List<SkillMatchDto> ruleBasedSkills)
		{
			var result = new List<SkillMatchDto>();
			var skillDict = new Dictionary<int, SkillMatchDto>();

			// Thêm AI skills trước (ưu tiên confidence cao hơn)
			foreach (var skill in aiSkills)
			{
				if (!skillDict.ContainsKey(skill.SkillId))
				{
					skillDict[skill.SkillId] = skill;
				}
			}

			// Thêm rule-based skills nếu chưa có (bổ sung kỹ năng AI bỏ sót)
			foreach (var skill in ruleBasedSkills)
			{
				if (!skillDict.ContainsKey(skill.SkillId))
				{
					skillDict[skill.SkillId] = skill;
					_logger.LogDebug("Added missing skill from rule-based: {SkillName}", skill.Name);
				}
			}

			return skillDict.Values.ToList();
		}

		private double CalculateConfidence(string text, Skill skill)
		{
			var skillName = skill.Name.ToLower();
			var occurrences = CountOccurrences(text, skillName);

			if (!string.IsNullOrWhiteSpace(skill.Aliases))
			{
				var aliases = skill.Aliases.Split(',');
				foreach (var alias in aliases)
				{
					occurrences += CountOccurrences(text, alias.Trim().ToLower());
				}
			}

			if (occurrences == 0) return 0;

			double baseScore = Math.Min(occurrences * 0.2, 0.8);

			// Bonus section Skills
			if (text.Contains("kỹ năng") && text.Contains(skillName))
				baseScore += 0.15;
			if (text.Contains("skills") && text.Contains(skillName))
				baseScore += 0.15;

			// Bonus section Experience
			if (text.Contains("experience") && text.Contains(skillName))
				baseScore += 0.1;
			if (text.Contains("kinh nghiệm") && text.Contains(skillName))
				baseScore += 0.1;

			// Bonus section Projects
			if (text.Contains("project") && text.Contains(skillName))
				baseScore += 0.05;

			// Bonus đầu CV
			var firstPart = text.Length > 0 ? text.Substring(0, Math.Min(text.Length / 5, text.Length)) : "";
			if (firstPart.Contains(skillName))
				baseScore += 0.1;

			return Math.Min(baseScore, 1.0);
		}

		private int CountOccurrences(string text, string word)
		{
			if (string.IsNullOrEmpty(word)) return 0;

			var count = 0;
			var index = 0;

			while ((index = text.IndexOf(word, index, StringComparison.Ordinal)) != -1)
			{
				count++;
				index += word.Length;
			}

			return count;
		}

		public async Task<AnalysisResultDto> GetAnalysisResultAsync(Guid cvId, Guid userId)
		{
			var cv = await _uow.CVs.GetByIdAsync(cvId);
			if (cv == null)
				throw new BusinessException(ErrorCode.CVNotFound, _msg.Business("CVNotFound"));

			if (cv.UserId != userId)
				throw new BusinessException(ErrorCode.Forbidden, _msg.Business("AccessDenied"));

			var result = new AnalysisResultDto
			{
				CvId = cv.Id,
				FileName = cv.FileName,
		 Status = (int)cv.Status,
			StatusName = cv.Status.ToString(),
			UploadedAt = cv.UploadedAt,
				AnalyzedAt = cv.AnalyzedAt,
				DownloadUrl = $"/api/v1/CV/{cv.Id}/download"
			};

			if (cv.Status == CVStatus.Pending || cv.Status == CVStatus.Uploaded)
			{
		  result.Status = (int)cv.Status;
			result.StatusName = MapStatus(cv.Status);
				result.Message = "This CV has not been analyzed yet";
			}
			else if (cv.Status == CVStatus.Processing)
			{
		   result.Status = (int)cv.Status;
			result.StatusName = MapStatus(cv.Status);
				result.Message = "CV is being analyzed. Estimated time: 5 seconds";
			}
			else if (cv.Status == CVStatus.Analyzed)
			{
			var analysisResults = await _uow.CVAnalysisResults.GetByCVIdAsync(cvId);
			result.Status = (int)cv.Status;
			result.StatusName = MapStatus(cv.Status);
				result.Skills = analysisResults.Select(r => new SkillMatchDto
				{
					SkillId = r.SkillId,
					Name = r.Skill?.Name ?? string.Empty,
					Category = r.Skill?.Category,
					Confidence = r.Confidence
				}).ToList();
				result.TotalSkills = result.Skills.Count;
			}
			else if (cv.Status == CVStatus.Failed)
			{
		   result.Status = (int)cv.Status;
			result.StatusName = MapStatus(cv.Status);
				result.Message = cv.ErrorMessage ?? "Analysis failed";
			}

			return result;
		}
	}
}