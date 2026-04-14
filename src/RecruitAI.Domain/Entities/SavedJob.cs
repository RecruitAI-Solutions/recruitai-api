namespace RecruitAI.Domain.Entities;

public class SavedJob
{
	public Guid Id { get; set; }
	public Guid JobId { get; set; }
	public Guid UserId { get; set; }
	public DateTime SavedAt { get; set; }

	// Navigation properties
	public virtual Job Job { get; set; } = null!;
	public virtual User User { get; set; } = null!;
}