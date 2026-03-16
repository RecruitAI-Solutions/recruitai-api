using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces;

namespace RecruitAI.Application.Queries.CVs;

public class GetUserCVsQuery : IRequest<IEnumerable<CV>>
{
	public Guid UserId { get; set; }
}

public class GetUserCVsQueryHandler : IRequestHandler<GetUserCVsQuery, IEnumerable<CV>>
{
	private readonly ICVRepository _cvRepository;
	private readonly ILogger<GetUserCVsQueryHandler> _logger;
	private readonly IMessageService _msg; 

	public GetUserCVsQueryHandler(
		ICVRepository cvRepository,
		ILogger<GetUserCVsQueryHandler> logger,
		IMessageService messageService)
	{
		_cvRepository = cvRepository;
		_logger = logger;
		_msg = messageService;
	}

	public async Task<IEnumerable<CV>> Handle(GetUserCVsQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation(_msg.Log("GettingUserCVs"), request.UserId);

		var cvs = await _cvRepository.GetByUserIdAsync(request.UserId);

		_logger.LogInformation(_msg.Log("FoundUserCVs"), cvs.Count(), request.UserId);

		return cvs;
	}
}