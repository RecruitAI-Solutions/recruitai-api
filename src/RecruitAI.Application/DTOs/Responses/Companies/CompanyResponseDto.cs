namespace RecruitAI.Application.DTOs.Responses.Companies;
// CompanyResponseDto - cho ứng viên xem
public class CompanyResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Slug { get; set; }
	public string? Logo { get; set; }
	public string? Address { get; set; }
	public string? Website { get; set; }
	public int TotalJobs { get; set; }
	public DateTime CreatedAt { get; set; }
}
