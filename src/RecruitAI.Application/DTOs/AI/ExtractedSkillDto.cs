namespace RecruitAI.Application.DTOs.AI
{
	public class ExtractedSkillDto
	{
		public string Name { get; set; } = string.Empty;
		public string? Category { get; set; }
		public double Confidence { get; set; }
	}
}