using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Entities;
using System.Text.Json;
using RecruitAI.Application.Events;

namespace RecruitAI.Application.Commands.Notifications;

public class CreateNotificationCommand : IRequest<Guid>
{
	public Guid UserId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string? Data { get; set; }
}

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Guid>
{
	private readonly IUnitOfWork _uow;
	private readonly IMediator _mediator; 
	private readonly ILogger<CreateNotificationCommandHandler> _logger;

	public CreateNotificationCommandHandler(
		IUnitOfWork uow,
		IMediator mediator,
		ILogger<CreateNotificationCommandHandler> logger)
	{
		_uow = uow;
		_mediator = mediator;
		_logger = logger;
	}

	public async Task<Guid> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
	{
		var notification = new Notification
		{
			Id = Guid.NewGuid(),
			UserId = request.UserId,
			Title = request.Title,
			Content = request.Content,
			Type = request.Type,
			IsRead = false,
			Data = request.Data,
			CreatedAt = DateTime.UtcNow
		};

		await _uow.Notifications.AddAsync(notification, cancellationToken);
		await _uow.SaveChangesAsync(cancellationToken);

		// Publish event
		await _mediator.Publish(new NotificationCreatedEvent
		{
			NotificationId = notification.Id,
			UserId = notification.UserId,
			Title = notification.Title,
			Content = notification.Content,
			Type = notification.Type,
			Data = notification.Data != null ? JsonSerializer.Deserialize<object>(notification.Data) : null,
			CreatedAt = notification.CreatedAt
		}, cancellationToken);

		return notification.Id;
	}
}
