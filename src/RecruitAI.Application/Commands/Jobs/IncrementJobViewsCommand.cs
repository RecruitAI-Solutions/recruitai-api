using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Commands.Jobs;

public class IncrementJobViewsCommand : IRequest
{
	public Guid Id { get; set; }
}

public class IncrementJobViewsCommandHandler : IRequestHandler<IncrementJobViewsCommand>
{
	private readonly IUnitOfWork _uow;
	private readonly ILogger<IncrementJobViewsCommandHandler> _logger;

	public IncrementJobViewsCommandHandler(IUnitOfWork uow, ILogger<IncrementJobViewsCommandHandler> logger)
	{
		_uow = uow;
		_logger = logger;
	}

	public async Task Handle(IncrementJobViewsCommand request, CancellationToken cancellationToken)
	{
		var job = await _uow.Jobs.GetByIdAsync(request.Id, cancellationToken);
		if (job != null)
		{
			job.IncrementViews();
			await _uow.SaveChangesAsync(cancellationToken);
			_logger.LogDebug("Incremented views for job {JobId} to {Views}", request.Id, job.Views);
		}
	}
}