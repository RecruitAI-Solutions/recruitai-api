namespace RecruitAI.Application.DTOs.Responses.Companies;
// CompanySuggestDto - cho autocomplete
public class CompanySuggestDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Logo { get; set; }
}