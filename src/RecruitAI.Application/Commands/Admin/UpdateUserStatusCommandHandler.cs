using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Commands.Admin
{
	public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, AdminUserStatusUpdateResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<UpdateUserStatusCommandHandler> _logger;

		public UpdateUserStatusCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateUserStatusCommandHandler> logger)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<AdminUserStatusUpdateResponseDto> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Admin updating user {UserId} status to {Status}", request.UserId, request.Status);

			var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

			if (user == null)
				throw new BusinessException(ErrorCode.UserNotFound, "User not found");

			user.Status = request.Status;
			user.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("User {UserId} status updated to {Status}", request.UserId, request.Status);

			return new AdminUserStatusUpdateResponseDto
			{
				Id = user.Id,
				Status = user.Status,
				Reason = request.Reason,
				UpdatedAt = user.UpdatedAt.Value
			};
		}
	}
}