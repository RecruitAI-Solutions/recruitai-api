namespace RecruitAI.Application.DTOs.AI;

public class AIRecommendationDto
{
	public List<string> Strengths { get; set; } = new();
	public List<string> Weaknesses { get; set; } = new();
	public List<string> Recommendations { get; set; } = new();
	public List<SuggestedCourseDto> SuggestedCourses { get; set; } = new();
	public string EstimatedCompetition { get; set; } = string.Empty;
	public string SuccessProbability { get; set; } = string.Empty;
}