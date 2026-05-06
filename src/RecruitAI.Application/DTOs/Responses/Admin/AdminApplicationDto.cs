using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecruitAI.Application.DTOs.Responses.Admin
{
	public class AdminApplicationDto
	{
		public Guid ApplicationId { get; set; }
		public Guid JobId { get; set; }
		public string JobTitle { get; set; } = string.Empty;
		public string JobLocation { get; set; } = string.Empty;
		public Guid RecruiterId { get; set; }
		public string RecruiterName { get; set; } = string.Empty;
		public string RecruiterEmail { get; set; } = string.Empty;

		public Guid CvId { get; set; }
		public string CvName { get; set; } = string.Empty;

		public Guid CandidateId { get; set; }
		public string CandidateName { get; set; } = string.Empty;
		public string CandidateEmail { get; set; } = string.Empty;
		public string? CandidatePhone { get; set; }

		public int MatchPercentage { get; set; }
		public int MatchedSkillCount { get; set; }
		public int RequiredSkillCount { get; set; }
		public List<string> MatchedSkills { get; set; } = new();
		public List<string> MissingSkills { get; set; } = new();

		public JobApplicationStatus Status { get; set; }
		public string StatusName { get; set; } = string.Empty;
		public string StatusDisplay { get; set; } = string.Empty;

		public DateTime AppliedAt { get; set; }
		public DateTime? ReviewedAt { get; set; }
		public string? Notes { get; set; }
	}

}
