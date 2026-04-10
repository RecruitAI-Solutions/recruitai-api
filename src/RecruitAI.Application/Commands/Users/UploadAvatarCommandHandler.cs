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
	public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, UploadAvatarResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IImageService _imageService;
		private readonly ILogger<UploadAvatarCommandHandler> _logger;
		private readonly IAuditLogService _auditLogService;
		private readonly IMessageService _msg;

		private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
		private const int MaxFileSize = 5 * 1024 * 1024; // 5MB

		public UploadAvatarCommandHandler(
			IUnitOfWork unitOfWork,
			IImageService imageService,
			ILogger<UploadAvatarCommandHandler> logger,
			IAuditLogService auditLogService,
			IMessageService msg)
		{
			_unitOfWork = unitOfWork;
			_imageService = imageService;
			_logger = logger;
			_auditLogService = auditLogService;
			_msg = msg;
		}

		public async Task<UploadAvatarResponseDto> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
			if (user == null)
				throw new BusinessException(ErrorCode.UserNotFound, "User not found");

			var file = request.Avatar;
			var fileExtension = Path.GetExtension(file.FileName).ToLower();

			if (!_allowedExtensions.Contains(fileExtension))
				throw new BusinessException(ErrorCode.InvalidFileType, _msg.Business("InvalidImageFormat"));

			if (file.Length > MaxFileSize)
				throw new BusinessException(ErrorCode.FileTooLarge, _msg.Business("FileTooLarge"));

			var oldAvatarUrl = user.AvatarUrl;

			var subDirectory = $"uploads/avatars/{request.UserId}";
			var (fileName, thumbnailFileName) = await _imageService.SaveImageAsync(file, subDirectory, cancellationToken);

			var avatarUrl = $"/{subDirectory}/{fileName}";
			var thumbnailUrl = $"/{subDirectory}/{thumbnailFileName}";

			if (!string.IsNullOrEmpty(oldAvatarUrl))
			{
				var oldFilePath = oldAvatarUrl.TrimStart('/');
				await _imageService.DeleteImageAsync(oldFilePath, cancellationToken);
			}

			user.AvatarUrl = avatarUrl;
			user.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var newValue = new Dictionary<string, string>
			{
				[_msg.Get("AuditFieldAvatarUrl")] = avatarUrl,
				[_msg.Get("AuditFieldThumbnailUrl")] = thumbnailUrl
			};

			await _auditLogService.LogAsync(
				AuditEntityType.User,
				AuditAction.Upload,
				user.Id.ToString(),
				user.Email,
				!string.IsNullOrEmpty(oldAvatarUrl) ? JsonSerializer.Serialize(new { oldAvatarUrl }) : null,
				JsonSerializer.Serialize(newValue),
				null,
				cancellationToken);

			_logger.LogInformation("User {UserId} uploaded new avatar", request.UserId);

			return new UploadAvatarResponseDto
			{
				AvatarUrl = avatarUrl,
				ThumbnailUrl = thumbnailUrl,
				UpdatedAt = user.UpdatedAt.Value
			};
		}
	}
}