using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.DTOs.Common
{
	public class JobSkill
	{
		public Guid JobId { get; set; }
		public int SkillId { get; set; }
		public bool IsRequired { get; set; }
		public Job Job { get; set; } = null!;
		public Skill Skill { get; set; } = null!;
	}

}
