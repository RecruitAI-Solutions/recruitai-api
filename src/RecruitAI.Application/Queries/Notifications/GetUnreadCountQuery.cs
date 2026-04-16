using MediatR;
using RecruitAI.Application.Interfaces;

namespace RecruitAI.Application.Queries.Notifications;

public class GetUnreadCountQuery : IRequest<int>
{
	public Guid UserId { get; set; }
}

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
	private readonly IUnitOfWork _uow;

	public GetUnreadCountQueryHandler(IUnitOfWork uow)
	{
		_uow = uow;
	}

	public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
	{
		return await _uow.Notifications.GetUnreadCountAsync(request.UserId, cancellationToken);
	}
}