// RecruitAI.Application/DTOs/Responses/AI/SkillMatchDto.cs
namespace RecruitAI.Application.DTOs.Responses.AI
{
	public class SkillMatchDto
	{
		public int SkillId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Category { get; set; }
		public double Confidence { get; set; }
	}
}