// RecruitAI.Application/Events/NotificationCreatedEvent.cs
using MediatR;

namespace RecruitAI.Application.Events;

public class NotificationCreatedEvent : INotification
{
	public Guid NotificationId { get; set; }
	public Guid UserId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public object? Data { get; set; }
	public DateTime CreatedAt { get; set; }
}