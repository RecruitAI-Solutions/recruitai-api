namespace RecruitAI.Domain.Entities;

public class Notification
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;  // application_update, job_match, system
	public bool IsRead { get; set; }
	public string? Data { get; set; }  // JSON data
	public DateTime CreatedAt { get; set; }

	// Navigation
	public virtual User User { get; set; } = null!;
}