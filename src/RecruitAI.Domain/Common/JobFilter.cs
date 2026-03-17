using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Common;

public class JobFilter
{
	public string? Title { get; set; }
	public string? Location { get; set; }
	public decimal? MinSalary { get; set; }
	public decimal? MaxSalary { get; set; }
	public EmploymentType? EmploymentType { get; set; }  
	public ExperienceLevel? ExperienceLevel { get; set; }
	public string? Skill { get; set; }
	public string? SortBy { get; set; }
	public string SortOrder { get; set; } = "desc";
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
}