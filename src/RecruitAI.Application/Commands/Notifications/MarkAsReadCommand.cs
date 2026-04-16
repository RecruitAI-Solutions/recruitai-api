using MediatR;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Commands.Notifications;

public class MarkAsReadCommand : IRequest<bool>
{
	public Guid NotificationId { get; set; }
	public Guid UserId { get; set; }
}

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, bool>
{
	private readonly IUnitOfWork _uow;

	public MarkAsReadCommandHandler(IUnitOfWork uow)
	{
		_uow = uow;
	}

	public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
	{
		await _uow.Notifications.MarkAsReadAsync(request.NotificationId, request.UserId, cancellationToken);
		await _uow.SaveChangesAsync(cancellationToken);

		return true;
	}
}