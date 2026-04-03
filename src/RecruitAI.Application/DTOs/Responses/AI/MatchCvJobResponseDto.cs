using RecruitAI.Application.DTOs.Common;

namespace RecruitAI.Application.DTOs.Responses.AI
{
	public class MatchCvJobResponseDto
	{
		public Guid CvId { get; set; }
		public Guid JobId { get; set; }
		public int MatchPercentage { get; set; }
		public int RequiredSkillCount { get; set; }
		public int MatchedSkillCount { get; set; }
		public List<SkillMatchDetailDto> MatchedSkills { get; set; } = new();
		public List<SkillMatchDetailDto> MissingSkills { get; set; } = new();
		public DateTime CalculatedAt { get; set; }
	}
}
