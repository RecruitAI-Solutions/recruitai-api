using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses.Users;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Text.Json;

namespace RecruitAI.Application.Commands.Users
{
	public class DeleteAvatarCommandHandler : IRequestHandler<DeleteAvatarCommand, DeleteAvatarResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IImageService _imageService;
		private readonly ILogger<DeleteAvatarCommandHandler> _logger;
		private readonly IAuditLogService _auditLogService;
		private readonly IMessageService _msg;
		public const string DefaultAvatarUrl = "/imgs/default_avatar/default.png";

		public DeleteAvatarCommandHandler(
			IUnitOfWork unitOfWork,
			IImageService imageService,
			ILogger<DeleteAvatarCommandHandler> logger,
			IAuditLogService auditLogService,
			IMessageService msg)
		{
			_unitOfWork = unitOfWork;
			_imageService = imageService;
			_logger = logger;
			_auditLogService = auditLogService;
			_msg = msg;
		}

		public async Task<DeleteAvatarResponseDto> Handle(DeleteAvatarCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
			if (user == null)
				throw new BusinessException(ErrorCode.UserNotFound, "User not found");

			var oldAvatarUrl = user.AvatarUrl;

			if (string.IsNullOrEmpty(oldAvatarUrl))
			{
				return new DeleteAvatarResponseDto
				{
					Message = _msg.Business("NoAvatarToDelete"),
					AvatarUrl = DeleteAvatarCommandHandler.DefaultAvatarUrl
				};
			}

			var subDirectory = $"uploads/avatars/{request.UserId}";
			await _imageService.DeleteDirectoryAsync(subDirectory, cancellationToken);

			user.AvatarUrl = null;
			user.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			await _auditLogService.LogAsync(
				AuditEntityType.User,
				AuditAction.Delete,
				user.Id.ToString(),
				user.Email,
				JsonSerializer.Serialize(new { oldAvatarUrl }),
				null,
				null,
				cancellationToken);

			_logger.LogInformation("User {UserId} deleted avatar", request.UserId);

			return new DeleteAvatarResponseDto
			{
				Message = _msg.Success("AvatarDeleted"),
				AvatarUrl = DeleteAvatarCommandHandler.DefaultAvatarUrl
			};
		}
	}
}