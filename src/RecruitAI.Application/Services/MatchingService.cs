using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.AI;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using System.Text.Json;

namespace RecruitAI.Application.Services
{
	public class MatchingService : IMatchingService
	{
		private readonly IUnitOfWork _unitOfWork;

		public MatchingService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<MatchCvJobResponseDto> CalculateAndSaveMatchAsync(Guid cvId, Guid jobId, Guid currentUserId)
		{
			// 1. Validate CV
			var cv = await _unitOfWork.CVs.GetByIdAsync(cvId);
			if (cv == null || cv.IsDeleted)
				throw new Exception("CV not found");

			// 2. Validate Job - GetByIdAsync đã include JobSkills
			var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
			if (job == null || job.IsDeleted || !job.IsActive)
				throw new Exception("Job not found or not active");

			// 3. Check permission
			var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
			if (currentUser == null)
				throw new Exception("User not found");

			bool isOwner = cv.UserId == currentUserId;
			bool isRecruiter = currentUser.Role == UserRole.RECRUITER;

			if (!isOwner && !isRecruiter)
				throw new UnauthorizedAccessException("You don't have permission to match this CV");

			// 4. Check CV has been analyzed
			var cvSkills = await _unitOfWork.CVAnalysisResults.GetByCvIdAsync(cvId);
			if (cvSkills == null || !cvSkills.Any())
				throw new Exception("CV has not been analyzed yet");

			// 5. Get job required skills
			var requiredSkills = job.JobSkills.Where(js => js.IsRequired).ToList();
			if (!requiredSkills.Any())
				throw new Exception("Job has no required skills");

			// 6. Calculate match
			var cvSkillIds = cvSkills.Select(cs => cs.SkillId).ToHashSet();
			var requiredSkillIds = requiredSkills.Select(rs => rs.SkillId).ToHashSet();

			var matchedSkillIds = cvSkillIds.Intersect(requiredSkillIds).ToList();
			var missingSkillIds = requiredSkillIds.Except(cvSkillIds).ToList();

			var matchedCount = matchedSkillIds.Count;
			var requiredCount = requiredSkillIds.Count;
			var matchPercentage = requiredCount > 0 ? (matchedCount * 100) / requiredCount : 0;

			// 7. Get skill details
			var allSkills = await _unitOfWork.Skills.GetAllAsync();
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
			var existingApplication = await _unitOfWork.JobApplications.GetByJobAndCvAsync(jobId, cvId);

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
				await _unitOfWork.JobApplications.AddAsync(existingApplication);
			}

			var existingMatch = await _unitOfWork.JobApplicationMatches.GetByApplicationIdAsync(existingApplication.Id);

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
				await _unitOfWork.JobApplicationMatches.AddAsync(existingMatch);
			}
			else
			{
				existingMatch.MatchPercentage = matchPercentage;
				existingMatch.RequiredSkillCount = requiredCount;
				existingMatch.MatchedSkillCount = matchedCount;
				existingMatch.MatchedSkillsJson = JsonSerializer.Serialize(matchedSkills);
				existingMatch.MissingSkillsJson = JsonSerializer.Serialize(missingSkills);
				existingMatch.CalculatedAt = DateTime.UtcNow;
				await _unitOfWork.JobApplicationMatches.UpdateAsync(existingMatch);
			}

			await _unitOfWork.SaveChangesAsync();

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

		public async Task<MatchCvJobResponseDto> GetMatchResultAsync(Guid cvId, Guid jobId, Guid currentUserId)
		{
			var cv = await _unitOfWork.CVs.GetByIdAsync(cvId);
			if (cv == null)
				throw new Exception("CV not found");

			var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
			bool isOwner = cv.UserId == currentUserId;
			bool isRecruiter = currentUser?.Role == UserRole.RECRUITER;

			if (!isOwner && !isRecruiter)
				throw new UnauthorizedAccessException("You don't have permission");

			var application = await _unitOfWork.JobApplications.GetByJobAndCvAsync(jobId, cvId);
			if (application == null)
				throw new Exception("Match not found. Please calculate match first.");

			var match = await _unitOfWork.JobApplicationMatches.GetByApplicationIdAsync(application.Id);
			if (match == null)
				throw new Exception("Match result not found");

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

		public async Task<CvMatchesListResponseDto> GetAllMatchesByCvIdAsync(Guid cvId, Guid currentUserId)
		{
			var cv = await _unitOfWork.CVs.GetByIdAsync(cvId);
			if (cv == null)
				throw new Exception("CV not found");

			var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
			bool isOwner = cv.UserId == currentUserId;
			bool isRecruiter = currentUser?.Role == UserRole.RECRUITER;

			if (!isOwner && !isRecruiter)
				throw new UnauthorizedAccessException("You don't have permission");

			var applications = await _unitOfWork.JobApplications.GetByCvIdAsync(cvId);

			var matches = new List<CvMatchSummaryDto>();

			foreach (var app in applications)
			{
				var match = await _unitOfWork.JobApplicationMatches.GetByApplicationIdAsync(app.Id);
				if (match != null)
				{
					// Load Job info nếu cần
					var job = await _unitOfWork.Jobs.GetByIdAsync(app.JobId);
					matches.Add(new CvMatchSummaryDto
					{
						JobId = app.JobId,
						JobTitle = job?.Title ?? "Unknown",
						Company = job?.Department ?? "Unknown",
						MatchPercentage = match.MatchPercentage,
						CalculatedAt = match.CalculatedAt
					});
				}
			}

			return new CvMatchesListResponseDto
			{
				CvId = cvId,
				Matches = matches.OrderByDescending(m => m.MatchPercentage).ToList()
			};
		}
	}
}