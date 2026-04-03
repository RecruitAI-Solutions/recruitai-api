using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Entities;

public class JobApplication
{
	public Guid Id { get; set; }
	public Guid JobId { get; set; }
	public Guid CVId { get; set; }
	public JobApplicationStatus Status { get; set; }
	public DateTime AppliedAt { get; set; }
	public DateTime? ReviewedAt { get; set; }
	public string? Notes { get; set; }

	// Navigation
	public virtual Job Job { get; set; } = null!;
	public virtual CV CV { get; set; } = null!;
	public virtual JobApplicationMatch? Match { get; set; }
}