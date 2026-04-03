// RecruitAI.Application/DTOs/Responses/AI/AnalyzeCvResponseDto.cs
namespace RecruitAI.Application.DTOs.Responses.AI
{
	public class AnalyzeCvResponseDto
	{
		public Guid CvId { get; set; }
		public string Status { get; set; } = string.Empty;
		public List<SkillMatchDto> Skills { get; set; } = new();
		public int TotalSkills { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string? Message { get; set; }
		public int? EstimatedTime { get; set; }
	}
}