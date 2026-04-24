using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Enums;
using RecruitAI.Infrastructure.Data;
using RecruitAI.Shared.DTOs;

namespace RecruitAI.Application.Queries.Candidate
{
	// GetCandidateDashboardQueryHandler.cs
	public class GetCandidateDashboardQueryHandler : IRequestHandler<GetCandidateDashboardQuery, CandidateDashboardDto>
	{
		private readonly RecruitDevContext _context;

		public GetCandidateDashboardQueryHandler(RecruitDevContext context)
		{
			_context = context;
		}

		public async Task<CandidateDashboardDto> Handle(GetCandidateDashboardQuery request, CancellationToken ct)
		{
			var today = DateTime.UtcNow.Date;
			var userId = request.UserId;

			// 1. Việc làm mới hôm nay
			var newJobsToday = await _context.Jobs
				.CountAsync(j => j.CreatedAt >= today && j.IsActive && !j.IsDeleted && j.Status == JobStatus.Published, ct);

			// 2. Đã ứng tuyển
			var totalApplications = await _context.JobApplications
				.CountAsync(a => a.CV.UserId == userId, ct);

			// 3. Tin phù hợp (lấy từ CV gần nhất)
			var latestCV = await _context.CVs
				.Where(c => c.UserId == userId && c.Status == CVStatus.Analyzed)
				.OrderByDescending(c => c.AnalyzedAt)
				.FirstOrDefaultAsync(ct);

			var suggestedJobs = 0;
			if (latestCV != null)
			{
				var cvSkillIds = await _context.CVAnalysisResult
					.Where(r => r.CVId == latestCV.Id)
					.Select(r => r.SkillId)
					.Distinct()
					.ToListAsync(ct);

				suggestedJobs = await _context.Jobs
					.Where(j => j.IsActive && !j.IsDeleted && j.Status == JobStatus.Published && j.ExpirationDate > DateTime.UtcNow)
					.Where(j => j.JobSkills.Any(js => cvSkillIds.Contains(js.SkillId)))
					.CountAsync(ct);
			}

			// 4. Đã được xem (đơn đã được review)
			var reviewedApplications = await _context.JobApplications
				.CountAsync(a => a.CV.UserId == userId && a.Status >= JobApplicationStatus.Reviewed, ct);

			return new CandidateDashboardDto
			{
				NewJobsToday = newJobsToday,
				TotalApplications = totalApplications,
				SuggestedJobs = suggestedJobs,
				ReviewedApplications = reviewedApplications
			};
		}
	}
}
