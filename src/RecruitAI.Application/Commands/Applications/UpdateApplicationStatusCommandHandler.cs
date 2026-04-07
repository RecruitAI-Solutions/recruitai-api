using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses.Jobs;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Commands.Applications;

public class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, UpdateApplicationStatusResponseDto>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<UpdateApplicationStatusCommandHandler> _logger;
	private readonly IMessageService _msg;

	public UpdateApplicationStatusCommandHandler(
		IUnitOfWork unitOfWork,
		ILogger<UpdateApplicationStatusCommandHandler> logger,
		IMessageService msg)
	{
		_unitOfWork = unitOfWork;
		_logger = logger;
		_msg = msg;
	}

	public async Task<UpdateApplicationStatusResponseDto> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
	{
		var application = await _unitOfWork.JobApplications.GetByIdAsync(request.ApplicationId, cancellationToken);
		if (application == null)
			_msg.Throw(ErrorCode.ResourceNotFound, "ApplicationNotFound");

		var job = await _unitOfWork.Jobs.GetByIdAsync(application.JobId, cancellationToken);
		if (job == null || job.RecruiterId != request.RecruiterId)
			_msg.Throw(ErrorCode.Forbidden, "NoPermissionToUpdateApplication");

		application.Status = request.Status;
		application.ReviewedAt = DateTime.UtcNow;

		if (!string.IsNullOrWhiteSpace(request.Notes))
			application.Notes = request.Notes;

		_unitOfWork.JobApplications.Update(application);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(_msg.Log("RecruiterUpdatedApplication"),
			request.RecruiterId, request.ApplicationId, request.Status);

		return new UpdateApplicationStatusResponseDto
		{
			ApplicationId = application.Id,
			Status = application.Status,
			Notes = application.Notes,
			UpdatedAt = application.ReviewedAt ?? DateTime.UtcNow
		};
	}
}