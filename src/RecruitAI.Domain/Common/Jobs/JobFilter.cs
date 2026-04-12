using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Common.Jobs;

public class JobFilter
{
	public string? Title { get; set; }
	public string? Location { get; set; }
	public decimal? MinSalary { get; set; }
	public decimal? MaxSalary { get; set; }
	public EmploymentType? EmploymentType { get; set; }  
	public ExperienceLevel? ExperienceLevel { get; set; }
	public string? Skill { get; set; }
     // Support multiple skills filtering
		public List<string>? Skills { get; set; }
		public bool MatchAllSkills { get; set; } = false;
	public string? SortBy { get; set; }
	public string SortOrder { get; set; } = "desc";
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
}