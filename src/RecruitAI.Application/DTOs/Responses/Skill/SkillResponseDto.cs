using System;

namespace RecruitAI.Application.DTOs.Responses.Skill
{
	public class SkillResponseDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Category { get; set; }
		public string? Aliases { get; set; }
		public string? ContextKeywords { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public string? CreatedBy { get; set; }
	}
}