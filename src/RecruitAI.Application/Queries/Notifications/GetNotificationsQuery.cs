using AutoMapper;
using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Notifications;
using RecruitAI.Application.Interfaces;

namespace RecruitAI.Application.Queries.Notifications;

public class GetNotificationsQuery : IRequest<PaginationResponseDto<NotificationResponseDto>>
{
	public Guid UserId { get; set; }
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
	public bool? IsRead { get; set; }
}

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PaginationResponseDto<NotificationResponseDto>>
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;

	public GetNotificationsQueryHandler(IUnitOfWork uow, IMapper mapper)
	{
		_uow = uow;
		_mapper = mapper;
	}

	public async Task<PaginationResponseDto<NotificationResponseDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
	{
		var (notifications, total) = await _uow.Notifications.GetByUserIdAsync(
			request.UserId, request.Page, request.PageSize, request.IsRead, cancellationToken);

		var items = _mapper.Map<List<NotificationResponseDto>>(notifications);

		return new PaginationResponseDto<NotificationResponseDto>
		{
			Data = items,
			Total = total,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}
}