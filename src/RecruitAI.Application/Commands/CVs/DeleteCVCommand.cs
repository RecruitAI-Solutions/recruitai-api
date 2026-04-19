// RecruitAI.Application/Commands/CVs/DeleteCVCommand.cs
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Text.Json;

namespace RecruitAI.Application.Commands.CVs;

public class DeleteCVCommand : IRequest
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }  // Người dùng hiện tại
}

public class DeleteCVCommandHandler : IRequestHandler<DeleteCVCommand>
{
	private readonly IUnitOfWork _uow;
	private readonly IConfiguration _configuration;
	private readonly IWebHostEnvironment _env;
	private readonly ILogger<DeleteCVCommandHandler> _logger;
	private readonly IAuditLogService _auditLogService;
	private readonly IMediator _mediator;
	private readonly IMessageService _msg;

	public DeleteCVCommandHandler(
		IUnitOfWork uow,
		IConfiguration configuration,
		IWebHostEnvironment env,
		ILogger<DeleteCVCommandHandler> logger,
		IAuditLogService auditLogService,
		IMediator mediator,
		IMessageService msg)
	{
		_uow = uow;
		_configuration = configuration;
		_env = env;
		_logger = logger;
		_auditLogService = auditLogService;
		_mediator = mediator;
		_msg = msg;
	}

	public async Task Handle(DeleteCVCommand request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Deleting CV {CvId} for user {UserId}", request.Id, request.UserId);

			var cv = await _uow.CVs.GetByIdAsync(request.Id);
			if (cv == null)
			{
				throw new BusinessException(ErrorCode.CVNotFound, "Không tìm thấy CV");
			}

			// Kiểm tra quyền sở hữu
			if (cv.UserId != request.UserId)
			{
				throw new BusinessException(ErrorCode.Forbidden, "Bạn không có quyền xóa CV này");
			}

			// Ghi audit log trước khi xóa
			await _auditLogService.LogAsync(
				AuditEntityType.CV,
				AuditAction.Delete,
				cv.Id.ToEntityId(),
				cv.FileName,
				null,
				null,
				null,
				cancellationToken);

			// Soft delete
			cv.IsDeleted = true;
			cv.DeletedAt = DateTime.UtcNow;

			await _uow.CVs.UpdateAsync(cv);
			await _uow.SaveChangesAsync(cancellationToken);

			// Xóa file vật lý dựa trên môi trường
			var filePath = GetPhysicalFilePath(cv.FilePath);
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				File.Delete(filePath);
				_logger.LogInformation("Deleted physical file: {FilePath}", filePath);
			}
			else
			{
				_logger.LogWarning("Physical file not found or invalid path: {FilePath}", filePath);
			}

			var notification = new CreateNotificationCommand
			{
				UserId = request.UserId,
				Title = _msg.Get("Notification.CVDeleted.Title"),
				Content = string.Format(_msg.Get("Notification.CVDeleted.Content"), cv.FileName),
				Type = "cv_update",
				Data = JsonSerializer.Serialize(new { CvId = cv.Id, FileName = cv.FileName })
			};
			await _mediator.Send(notification, cancellationToken);

			_logger.LogInformation("Notification sent to user {UserId} for CV deletion", request.UserId);

			_logger.LogInformation("CV {CvId} deleted successfully", request.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting CV {CvId}", request.Id);
			throw;
		}
	}

	private string GetPhysicalFilePath(string relativePath)
	{
		if (string.IsNullOrEmpty(relativePath))
		{
			_logger.LogWarning("Relative path is null or empty");
			return null;
		}

		var useSeparatePath = _configuration.GetValue<bool>("FileStorage:UseSeparateUploadPath", false);

		if (useSeparatePath)
		{
			// PRODUCTION: Dùng thư mục riêng
			var uploadRoot = _configuration["FileStorage:UploadRootPath"];

			if (string.IsNullOrEmpty(uploadRoot))
			{
				_logger.LogWarning("Upload root path not configured for Production");
				return null;
			}

			return Path.Combine(uploadRoot, relativePath);
		}
		else
		{
			// DEVELOPMENT: Dùng wwwroot
			if (string.IsNullOrEmpty(_env.WebRootPath))
			{
				_logger.LogWarning("Web root path not configured for Development");
				return null;
			}

			return Path.Combine(_env.WebRootPath, relativePath);
		}
	}
}