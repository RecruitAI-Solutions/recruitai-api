// RecruitAI.Application/Queries/Jobs/GetJobSuggestionsByCVQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Domain.Enums;
using RecruitAI.Infrastructure.Data;
using RecruitAI.Shared.DTOs;

namespace RecruitAI.Application.Queries.Jobs
{
	public class GetJobSuggestionsByCVQueryHandler : IRequestHandler<GetJobSuggestionsByCVQuery, PaginationResponseDto<JobMatchResultDto>>
	{
		private readonly RecruitDevContext _context;
		private readonly DateTime _now = DateTime.UtcNow;

		public GetJobSuggestionsByCVQueryHandler(RecruitDevContext context)
		{
			_context = context;
		}

		public async Task<PaginationResponseDto<JobMatchResultDto>> Handle(GetJobSuggestionsByCVQuery request, CancellationToken cancellationToken)
		{
			// 1. Lấy skill IDs từ CV
			var cvSkillIds = await _context.CVAnalysisResult
				.Where(r => r.CVId == request.CVId)
				.Select(r => r.SkillId)
				.Distinct()
				.ToListAsync(cancellationToken);

			if (!cvSkillIds.Any())
			{
				return new PaginationResponseDto<JobMatchResultDto>
				{
					Data = new List<JobMatchResultDto>(),
					Total = 0,
					Page = request.Page,
					PageSize = request.PageSize
				};
			}

			// 2. Lấy danh sách job active và tính match count
			var allJobs = await _context.Jobs
				.Include(j => j.Company)
				.Include(j => j.JobSkills)
				.ThenInclude(js => js.Skill)
				.Where(j => j.Status == JobStatus.Published && !j.IsDeleted && j.ExpirationDate > _now)
				.Distinct()
				.ToListAsync(cancellationToken);

			var matchedJobs = new List<JobMatchResultDto>();

			foreach (var job in allJobs)
			{
				var jobSkillIds = job.JobSkills.Select(js => js.SkillId).ToList();
				var matchedSkillIds = jobSkillIds.Intersect(cvSkillIds).ToList();
				var matchedCount = matchedSkillIds.Count;

				if (matchedCount >= request.MinMatchSkills)
				{
					var allRequiredSkillIds = job.JobSkills.Where(js => js.IsRequired).Select(js => js.SkillId).ToList();
					var missingIds = allRequiredSkillIds.Except(matchedSkillIds).ToList();

					var matchedSkillNames = await _context.Skills
						.Where(s => matchedSkillIds.Contains(s.Id))
						.Select(s => s.Name)
						.ToListAsync(cancellationToken);

					var missingSkillNames = await _context.Skills
						.Where(s => missingIds.Contains(s.Id))
						.Select(s => s.Name)
						.ToListAsync(cancellationToken);

					matchedJobs.Add(new JobMatchResultDto
					{
						Id = job.Id,
						Title = job.Title,
						CompanyName = job.Company?.Name ?? string.Empty,
						Location = job.Location,
						SalaryDisplay = FormatSalary(job.SalaryMin, job.SalaryMax, job.Currency),
						EmploymentType = job.EmploymentType?.ToString() ?? string.Empty,
						ExperienceLevel = job.ExperienceLevel?.ToString() ?? string.Empty,
						MatchedSkillCount = matchedCount,
						TotalRequiredSkills = allRequiredSkillIds.Count,
						MatchPercentage = allRequiredSkillIds.Count > 0 ? matchedCount * 100 / allRequiredSkillIds.Count : 0,
						MatchedSkills = matchedSkillNames,
						MissingSkills = missingSkillNames,
						CreatedAt = job.CreatedAt,
						Views = job.Views,
						Applications = job.Applications
					});
				}
			}

			// Sắp xếp và phân trang
			var total = matchedJobs.Count;
			var pagedJobs = matchedJobs
				.OrderByDescending(x => x.MatchedSkillCount)
				.Skip((request.Page - 1) * request.PageSize)
				.Take(request.PageSize)
				.ToList();

			return new PaginationResponseDto<JobMatchResultDto>
			{
				Data = pagedJobs,
				Total = total,
				Page = request.Page,
				PageSize = request.PageSize
			};
		}

		private string FormatSalary(decimal? min, decimal? max, Currency currency)
		{
			if (!min.HasValue && !max.HasValue) return "Thỏa thuận";
			var symbol = currency == Currency.VND ? "₫" : "$";
			if (min.HasValue && max.HasValue) return $"{min.Value:N0} - {max.Value:N0} {symbol}";
			if (min.HasValue) return $"{min.Value:N0}+ {symbol}";
			return $"Tối đa {max.Value:N0} {symbol}";
		}
	}
}