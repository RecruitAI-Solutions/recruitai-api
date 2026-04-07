using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Commands.Admin
{
	public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, AdminUserDeleteResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<DeleteUserCommandHandler> _logger;

		public DeleteUserCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteUserCommandHandler> logger)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<AdminUserDeleteResponseDto> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Admin {AdminId} deleting user {UserId}", request.CurrentAdminId, request.UserId);

			// Cannot delete yourself
			if (request.UserId == request.CurrentAdminId)
				throw new BusinessException(ErrorCode.InvalidData, "Cannot delete your own account");

			var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

			if (user == null)
				throw new BusinessException(ErrorCode.UserNotFound, "User not found");

			await _unitOfWork.Users.SoftDeleteAsync(request.UserId, cancellationToken);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("User {UserId} deleted by admin {AdminId}", request.UserId, request.CurrentAdminId);

			return new AdminUserDeleteResponseDto
			{
				Id = request.UserId,
				Deleted = true,
				DeletedAt = DateTime.UtcNow
			};
		}
	}
}