using MediatR;
using RecruitAI.Application.Interfaces;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.Application.Commands.Notifications;

public class MarkAllAsReadCommand : IRequest<bool>
{
	public Guid UserId { get; set; }
}

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, bool>
{
	private readonly IUnitOfWork _uow;

	public MarkAllAsReadCommandHandler(IUnitOfWork uow)
	{
		_uow = uow;
	}

	public async Task<bool> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
	{
		await _uow.Notifications.MarkAllAsReadAsync(request.UserId, cancellationToken);
		await _uow.SaveChangesAsync(cancellationToken);

		return true;
	}
}