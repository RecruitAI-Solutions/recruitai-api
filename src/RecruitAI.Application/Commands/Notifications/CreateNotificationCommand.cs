using MediatR;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Entities;

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

	public CreateNotificationCommandHandler(IUnitOfWork uow)
	{
		_uow = uow;
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

		return notification.Id;
	}
}