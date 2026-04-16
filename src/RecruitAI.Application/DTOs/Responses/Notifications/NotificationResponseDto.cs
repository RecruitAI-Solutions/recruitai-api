namespace RecruitAI.Application.DTOs.Responses.Notifications;

public class NotificationResponseDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public bool IsRead { get; set; }
	public string? Data { get; set; }
	public DateTime CreatedAt { get; set; }
}