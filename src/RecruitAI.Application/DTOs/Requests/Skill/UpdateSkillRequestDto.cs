using System.ComponentModel.DataAnnotations;

namespace RecruitAI.Application.DTOs.Requests.Skill
{
	public class UpdateSkillRequestDto
	{
		[Required]
		[MaxLength(100)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(50)]
		public string? Category { get; set; }

		[MaxLength(500)]
		public string? Aliases { get; set; }

		[MaxLength(500)]
		public string? ContextKeywords { get; set; }

		public bool IsActive { get; set; }
	}
}