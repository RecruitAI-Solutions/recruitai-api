namespace RecruitAI.Domain.Common.Skills
{
	public class SkillMapping
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Category { get; set; }
		public bool IsRequired { get; set; }
	}
}