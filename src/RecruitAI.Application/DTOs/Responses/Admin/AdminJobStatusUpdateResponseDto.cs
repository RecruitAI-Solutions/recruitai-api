using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecruitAI.Application.DTOs.Responses.Admin
{
	public class AdminJobStatusUpdateResponseDto
	{
		public Guid JobId { get; set; }
		public string JobTitle { get; set; } = string.Empty;
		public JobStatus OldStatus { get; set; }
		public string OldStatusName { get; set; } = string.Empty;
		public JobStatus NewStatus { get; set; }
		public string NewStatusName { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public DateTime UpdatedAt { get; set; }
		public bool Success { get; set; }
		public string? Message { get; set; }
	}
}
