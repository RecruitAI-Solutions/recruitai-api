using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Requests.AI;
using RecruitAI.Application.DTOs.Responses.AI;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Domain.Interfaces.Services;

namespace RecruitAI.Application.Services
{
	public class AnalysisService : IAnalysisService
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<AnalysisService> _logger;
		private readonly IMessageService _msg;
		private readonly IPdfService _pdfService; 

		public AnalysisService(
			IUnitOfWork uow,
			ILogger<AnalysisService> logger,
			IMessageService msg,
			IPdfService pdfService) 
		{
			_uow = uow;
			_logger = logger;
			_msg = msg;
			_pdfService = pdfService;  
		}

		public async Task<AnalyzeCvResponseDto> AnalyzeCVAsync(AnalyzeCvRequestDto request, Guid userId)
		{
			var cv = await _uow.CVs.GetByIdAsync(request.CvId);
			if (cv == null)
				throw new BusinessException(ErrorCode.CVNotFound, _msg.Business("CVNotFound"));

			if (cv.UserId != userId)
				throw new BusinessException(ErrorCode.Forbidden, _msg.Business("AccessDenied"));

			// Cho phép re-analyze
			if (cv.Status == CVStatus.Analyzed)
			{
				await _uow.CVAnalysisResults.RemoveByCVIdAsync(cv.Id);
				cv.Status = CVStatus.Completed;
				cv.AnalyzedAt = null;
				await _uow.SaveChangesAsync();
			}

			// Thử extract text nếu chưa có
			if (string.IsNullOrWhiteSpace(cv.ExtractedText))
			{
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cv.FilePath);
				if (File.Exists(filePath))
				{
					_logger.LogInformation("Extracting text from PDF file: {FilePath}", filePath);
					cv.ExtractedText = await _pdfService.ExtractTextAsync(filePath);
					cv.Status = CVStatus.Completed;
					await _uow.SaveChangesAsync();
				}
				else
				{
					throw new BusinessException(ErrorCode.InvalidData, "CV has no extracted text and PDF file not found");
				}
			}

			if (cv.Status != CVStatus.Completed)
			{
				return new AnalyzeCvResponseDto
				{
					CvId = cv.Id,
					Status = "processing",
					Message = "CV is being analyzed. Please check back later.",
					EstimatedTime = 5
				};
			}

			var allSkills = await _uow.Skills.GetAllActiveAsync();
			var matchedSkills = new List<SkillMatchDto>();
			var cvText = cv.ExtractedText.ToLower();

			foreach (var skill in allSkills)
			{
				var confidence = CalculateConfidence(cvText, skill);
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

			var analysisResults = matchedSkills.Select(s => new CVAnalysisResult
			{
				CVId = cv.Id,
				SkillId = s.SkillId,
				Confidence = s.Confidence,
				CreatedAt = DateTime.UtcNow
			});

			await _uow.CVAnalysisResults.RemoveByCVIdAsync(cv.Id);
			await _uow.CVAnalysisResults.AddRangeAsync(analysisResults);

			cv.Status = CVStatus.Analyzed;
			cv.AnalyzedAt = DateTime.UtcNow;
			await _uow.CVs.UpdateAsync(cv);
			await _uow.SaveChangesAsync();

			return new AnalyzeCvResponseDto
			{
				CvId = cv.Id,
				Status = "completed",
				Skills = matchedSkills.OrderByDescending(s => s.Confidence).ToList(),
				TotalSkills = matchedSkills.Count,
				ProcessedAt = DateTime.UtcNow
			};
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
				UploadedAt = cv.UploadedAt,
				AnalyzedAt = cv.AnalyzedAt,
				DownloadUrl = $"/api/v1/CV/{cv.Id}/download"
			};

			if (cv.Status == CVStatus.Pending || cv.Status == CVStatus.Uploaded)
			{
				result.Status = "pending";
				result.Message = "This CV has not been analyzed yet";
			}
			else if (cv.Status == CVStatus.Processing)
			{
				result.Status = "processing";
				result.Message = "CV is being analyzed. Estimated time: 5 seconds";
			}
			else if (cv.Status == CVStatus.Analyzed)
			{
				var analysisResults = await _uow.CVAnalysisResults.GetByCVIdAsync(cvId);
				result.Status = "analyzed";
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
				result.Status = "failed";
				result.Message = cv.ErrorMessage ?? "Analysis failed";
			}

			return result;
		}

		private double CalculateConfidence(string text, Skill skill)
		{
			var skillName = skill.Name.ToLower();
			var occurrences = 0;

			occurrences += CountOccurrences(text, skillName);

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
	}
}