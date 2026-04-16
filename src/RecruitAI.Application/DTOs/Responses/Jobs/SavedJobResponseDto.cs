namespace RecruitAI.Application.DTOs.Responses.Jobs;

public class SavedJobResponseDto
{
	public Guid JobId { get; set; }
	public string JobTitle { get; set; } = string.Empty;
	public string? CompanyName { get; set; }
	public string? CompanyLogo { get; set; }
	public string Location { get; set; } = string.Empty;
	public decimal? SalaryMin { get; set; }
	public decimal? SalaryMax { get; set; }
	public List<string> SkillNames { get; set; } = new();
	public DateTime SavedAt { get; set; }
}