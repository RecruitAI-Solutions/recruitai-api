// RecruitAI.Domain/Entities/CVAnalysisResult.cs
namespace RecruitAI.Domain.Entities
{
	public class CVAnalysisResult
	{
		private double _confidence;

		public Guid Id { get; set; }
		public Guid CVId { get; set; }
		public int SkillId { get; set; }
		public double Confidence { get; set; }
		public DateTime CreatedAt { get; set; }

		// Navigation properties
		public virtual CV CV { get; set; } = null!;
		public virtual Skill Skill { get; set; } = null!;
		public double Confidence
		{
			get => _confidence;
			set => _confidence = Math.Round(value, 2); // làm tròn 2 chữ số thập phân
		}

	}
}