using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Commands.Admin
{
	public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, AdminUserDeleteResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<DeleteUserCommandHandler> _logger;
		private readonly IAuditLogService _auditLogService;  

		public DeleteUserCommandHandler(
			IUnitOfWork unitOfWork,
			ILogger<DeleteUserCommandHandler> logger,
			IAuditLogService auditLogService)  
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_auditLogService = auditLogService;
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

			// Ghi audit log trước khi xóa
			await _auditLogService.LogAsync(
				AuditEntityType.User,
				AuditAction.Delete,
				user.Id.ToEntityId(),
				user.Email,
				null,
				null,
				null,
				cancellationToken);

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