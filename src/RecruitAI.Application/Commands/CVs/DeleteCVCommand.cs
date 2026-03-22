// RecruitAI.Application/Commands/CVs/DeleteCVCommand.cs
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.Application.Commands.CVs;

public class DeleteCVCommand : IRequest
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }  // Người dùng hiện tại
}

public class DeleteCVCommandHandler : IRequestHandler<DeleteCVCommand>
{
	private readonly IUnitOfWork _uow;
	private readonly ILogger<DeleteCVCommandHandler> _logger;

	public DeleteCVCommandHandler(IUnitOfWork uow, ILogger<DeleteCVCommandHandler> logger)
	{
		_uow = uow;
		_logger = logger;
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

			// Soft delete
			cv.IsDeleted = true;
			cv.DeletedAt = DateTime.UtcNow;

			await _uow.CVs.UpdateAsync(cv);
			await _uow.SaveChangesAsync(cancellationToken);

			// Xóa file vật lý (tùy chọn)
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cv.FilePath);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			_logger.LogInformation("CV {CvId} deleted successfully", request.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting CV {CvId}", request.Id);
			throw;
		}
	}
}