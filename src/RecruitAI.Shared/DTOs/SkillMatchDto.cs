namespace RecruitAI.Shared.DTOs
{
	public class SkillMatchDto
	{
		public int SkillId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Category { get; set; }
		public double Confidence { get; set; }
	}
}