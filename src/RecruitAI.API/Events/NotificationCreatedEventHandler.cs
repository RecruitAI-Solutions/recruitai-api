// RecruitAI.API/Events/NotificationCreatedEventHandler.cs
using MediatR;
using Microsoft.AspNetCore.SignalR;
using RecruitAI.API.Hubs;
using RecruitAI.Application.Events;

namespace RecruitAI.API.Events;

public class NotificationCreatedEventHandler : INotificationHandler<NotificationCreatedEvent>
{
	private readonly IHubContext<NotificationHub> _hubContext;
	private readonly ILogger<NotificationCreatedEventHandler> _logger;

	public NotificationCreatedEventHandler(
		IHubContext<NotificationHub> hubContext,
		ILogger<NotificationCreatedEventHandler> logger)
	{
		_hubContext = hubContext;
		_logger = logger;
	}

	public async Task Handle(NotificationCreatedEvent notification, CancellationToken cancellationToken)
	{
		try
		{
			var notificationDto = new
			{
				notification.NotificationId,
				notification.Title,
				notification.Content,
				notification.Type,
				notification.CreatedAt,
				IsRead = false,
				Data = notification.Data
			};

			await _hubContext.Clients.User(notification.UserId.ToString())
				.SendAsync("ReceiveNotification", notificationDto, cancellationToken);

			_logger.LogInformation("Real-time notification sent to user {UserId}", notification.UserId);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to send real-time notification to user {UserId}", notification.UserId);
		}
	}
}