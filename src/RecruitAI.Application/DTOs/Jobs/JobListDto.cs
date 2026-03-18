using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Jobs;

public class JobListDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
	public decimal? SalaryMin { get; set; }
	public decimal? SalaryMax { get; set; }
	public Currency Currency { get; set; }
	public EmploymentType EmploymentType { get; set; }
	public ExperienceLevel ExperienceLevel { get; set; }
	public string RecruiterName { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public DateTime ExpirationDate { get; set; }
	public bool IsActive { get; set; }

	public List<string> SkillNames { get; set; } = new();
}