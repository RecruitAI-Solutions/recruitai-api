using MediatR;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Domain.Interfaces;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.CVs;

public class GetCVByIdQuery : IRequest<CV?>
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
}

public class GetCVByIdQueryHandler : IRequestHandler<GetCVByIdQuery, CV?>
{
	private readonly IUnitOfWork _uow;
	private readonly IMessageService _msg;

	public GetCVByIdQueryHandler(
		IUnitOfWork uow,
		IMessageService messageService)
	{
		_uow = uow;
		_msg = messageService;
	}

	public async Task<CV?> Handle(GetCVByIdQuery request, CancellationToken cancellationToken)
	{
		var cv = await _uow.CVs.GetByIdAsync(request.Id);

		// Kiểm tra CV có tồn tại không
		if (cv == null)
		{
			throw new BusinessException(
				ErrorCode.CVNotFound,
				_msg.Business("CVNotFound"));
		}

		// Kiểm tra quyền sở hữu
		if (cv.UserId != request.UserId)
		{
			throw new BusinessException(
				ErrorCode.Forbidden,
				_msg.Business("AccessDenied"));
		}

		return cv;
	}
}