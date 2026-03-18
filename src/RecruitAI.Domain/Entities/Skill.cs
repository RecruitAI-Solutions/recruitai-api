using System;

namespace RecruitAI.Domain.Entities
{
	public class Skill
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Category { get; set; }
		public string? Aliases { get; set; }
		public string? ContextKeywords { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public string? CreatedBy { get; set; }
		public string? UpdatedBy { get; set; }
		public virtual ICollection<JobSkill> JobSkills { get; set; } = new HashSet<JobSkill>();
	}
}