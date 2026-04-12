// RecruitAI.Application/DTOs/Responses/AI/AnalysisResultDto.cs
namespace RecruitAI.Application.DTOs.Responses.AI
{
	public class AnalysisResultDto
	{
		public Guid CvId { get; set; }
		public string FileName { get; set; } = string.Empty;
      // Numeric status code
		public int Status { get; set; }

		// Name of the status
		public string StatusName { get; set; } = string.Empty;
		public DateTime? UploadedAt { get; set; }
		public DateTime? AnalyzedAt { get; set; }
		public List<SkillMatchDto> Skills { get; set; } = new();
		public int TotalSkills { get; set; }
		public string? Message { get; set; }
		public int? EstimatedTime { get; set; }
		public string? DownloadUrl { get; set; }
	}
}