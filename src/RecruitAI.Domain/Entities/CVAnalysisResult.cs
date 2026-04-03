// RecruitAI.Domain/Entities/CVAnalysisResult.cs
namespace RecruitAI.Domain.Entities
{
	public class CVAnalysisResult
	{
		public Guid Id { get; set; }
		public Guid CVId { get; set; }
		public int SkillId { get; set; }
		public double Confidence { get; set; }
		public DateTime CreatedAt { get; set; }

		// Navigation properties
		public virtual CV CV { get; set; } = null!;
		public virtual Skill Skill { get; set; } = null!;
	}
}