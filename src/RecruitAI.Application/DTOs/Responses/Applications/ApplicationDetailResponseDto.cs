using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Applications
{
	public class ApplicationDetailResponseDto
	{
		public Guid Id { get; set; }
		public JobInfoDto Job { get; set; } = new();
		public CvInfoDto Cv { get; set; } = new();
		public CandidateInfoDto Candidate { get; set; } = new();
		public MatchResultDto? MatchResult { get; set; }
            public JobApplicationStatus Status { get; set; }
			public string StatusName { get; set; } = string.Empty;
			public string? StatusDisplay { get; set; }
		public DateTime AppliedAt { get; set; }
		public DateTime? ReviewedAt { get; set; }
		public string? Notes { get; set; }
	}
}