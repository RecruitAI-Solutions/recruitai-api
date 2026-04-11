namespace RecruitAI.Application.DTOs.AI;

public class SuggestedCourseDto
{
	public string Name { get; set; } = string.Empty;
	public string Platform { get; set; } = string.Empty;
	public string Url { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public string? Currency { get; set; } = "VND";
}