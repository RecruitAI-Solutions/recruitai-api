using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Commands.Jobs;

public class SaveJobCommand : IRequest<bool>
{
	public Guid JobId { get; set; }
	public Guid UserId { get; set; }
}

public class SaveJobCommandHandler : IRequestHandler<SaveJobCommand, bool>
{
	private readonly IUnitOfWork _uow;
	private readonly ILogger<SaveJobCommandHandler> _logger;
	private readonly IMessageService _msg;

	public SaveJobCommandHandler(IUnitOfWork uow, ILogger<SaveJobCommandHandler> logger, IMessageService message)
	{
		_uow = uow;
		_logger = logger;
		_msg = message;
	}

	public async Task<bool> Handle(SaveJobCommand request, CancellationToken cancellationToken)
	{
		var job = await _uow.Jobs.GetByIdAsync(request.JobId, cancellationToken);
		if (job == null || job.IsDeleted || !job.IsActive || job.IsExpired())
			throw new BusinessException(ErrorCode.ResourceNotFound, _msg.Business("JobNotFoundOrExpired"));

		var existing = await _uow.SavedJobs.GetByJobAndUserAsync(request.JobId, request.UserId, cancellationToken);
		if (existing != null)
			throw new BusinessException(ErrorCode.DuplicateEntry, _msg.Business("SaveJobAlreadyExists"));

		var savedJob = new SavedJob
		{
			Id = Guid.NewGuid(),
			JobId = request.JobId,
			UserId = request.UserId,
			SavedAt = DateTime.UtcNow
		};

		await _uow.SavedJobs.AddAsync(savedJob, cancellationToken);
		await _uow.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("User {UserId} saved job {JobId}", request.UserId, request.JobId);

		return true;
	}
}