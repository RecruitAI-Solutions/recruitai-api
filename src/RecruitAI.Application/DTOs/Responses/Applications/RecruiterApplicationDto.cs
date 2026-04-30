using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecruitAI.Application.DTOs.Responses.Applications
{
	public class RecruiterApplicationDto
	{
		public Guid ApplicationId { get; set; }
		public Guid JobId { get; set; }
		public string JobTitle { get; set; } = string.Empty;
		public string JobLocation { get; set; } = string.Empty;

		// Candidate info
		public Guid CandidateId { get; set; }
		public string CandidateName { get; set; } = string.Empty;
		public string CandidateEmail { get; set; } = string.Empty;
		public string? CandidatePhone { get; set; }

		// CV info
		public Guid CvId { get; set; }
		public string CvName { get; set; } = string.Empty;
		public string CvDownloadUrl { get; set; } = string.Empty;

		// Match result
		public int MatchPercentage { get; set; }
		public int MatchedSkillCount { get; set; }
		public int RequiredSkillCount { get; set; }
		public List<string> MatchedSkills { get; set; } = new();
		public List<string> MissingSkills { get; set; } = new();

		// Application status
		public JobApplicationStatus Status { get; set; }
		public string StatusName { get; set; } = string.Empty;
		public string StatusDisplay { get; set; } = string.Empty;

		// Timestamps
		public DateTime AppliedAt { get; set; }
		public DateTime? ReviewedAt { get; set; }
		public string? Notes { get; set; }
	}
}
