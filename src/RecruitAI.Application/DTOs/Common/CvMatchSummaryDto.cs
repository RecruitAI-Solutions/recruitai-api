using System;
using System.Collections.Generic;
using System.Text;

namespace RecruitAI.Application.DTOs.Common
{
	public class CvMatchSummaryDto
	{
		public Guid JobId { get; set; }
		public string JobTitle { get; set; } = string.Empty;
		public string Company { get; set; } = string.Empty;
		public int MatchPercentage { get; set; }
		public DateTime CalculatedAt { get; set; }
	}
}
