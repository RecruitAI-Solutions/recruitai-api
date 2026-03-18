namespace RecruitAI.Domain.Entities
{
	public class JobSkill
	{
		public Guid JobId { get; set; }
		public int SkillId { get; set; }
		public bool IsRequired { get; set; } = true;

		// Navigation properties
		public virtual Job Job { get; set; } = null!;
		public virtual Skill Skill { get; set; } = null!;
	}
}