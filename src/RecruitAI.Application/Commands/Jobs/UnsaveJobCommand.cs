using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Application.Commands.Jobs;

public class UnsaveJobCommand : IRequest<bool>
{
	public Guid JobId { get; set; }
	public Guid UserId { get; set; }
}

public class UnsaveJobCommandHandler : IRequestHandler<UnsaveJobCommand, bool>
{
	private readonly IUnitOfWork _uow;
	private readonly ILogger<UnsaveJobCommandHandler> _logger;
	private readonly IMessageService _msg;

	public UnsaveJobCommandHandler(IUnitOfWork uow, ILogger<UnsaveJobCommandHandler> logger, IMessageService message)
	{
		_uow = uow;
		_logger = logger;
		_msg = message;
	}

	public async Task<bool> Handle(UnsaveJobCommand request, CancellationToken cancellationToken)
	{
		var savedJob = await _uow.SavedJobs.GetByJobAndUserAsync(request.JobId, request.UserId, cancellationToken);
		if (savedJob == null)
			throw new BusinessException(ErrorCode.ResourceNotFound, _msg.Business("JobNotFound"));

		_uow.SavedJobs.Remove(savedJob);
		await _uow.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("User {UserId} unsaved job {JobId}", request.UserId, request.JobId);

		return true;
	}
}