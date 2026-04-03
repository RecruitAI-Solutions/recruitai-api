using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Domain.Interfaces;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Interfaces.Services;

namespace RecruitAI.Application.Commands.CVs;

public class UploadCVCommand : IRequest<UploadCVResponseDto>
{
	public Guid UserId { get; set; }
	public string FileName { get; set; } = string.Empty;
	public Stream FileStream { get; set; } = null!;
	public long FileSize { get; set; }
	public string ContentType { get; set; } = string.Empty;
}

public class UploadCVCommandHandler : IRequestHandler<UploadCVCommand, UploadCVResponseDto>
{
	private readonly IUnitOfWork _uow;
	private readonly IWebHostEnvironment _env;
	private readonly ILogger<UploadCVCommandHandler> _logger;
	private readonly IMessageService _msg;
	private readonly IPdfService _pdfService;

	public UploadCVCommandHandler(
		IUnitOfWork uow,
		IWebHostEnvironment env,
		ILogger<UploadCVCommandHandler> logger,
		IMessageService messageService,
		IPdfService pdfService)
	{
		_uow = uow;
		_env = env;
		_logger = logger;
		_msg = messageService;
		_pdfService = pdfService;
	}

	public async Task<UploadCVResponseDto> Handle(UploadCVCommand request, CancellationToken cancellationToken)
	{
		try
		{
			// KIỂM TRA VALIDATION
			if (request.FileSize <= 0)
			{
				throw new BusinessException(
					ErrorCode.InvalidFile,
					_msg.Business("EmptyFile")); 
			}

			if (request.FileSize > 10 * 1024 * 1024) // 10MB
			{
				throw new BusinessException(
					ErrorCode.FileTooLarge,
					_msg.Business("FileTooLarge"));  
			}

			if (!request.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
			{
				throw new BusinessException(
					ErrorCode.InvalidFileType,
					_msg.Business("OnlyPdfAllowed"));  
			}

			var cvId = Guid.NewGuid();

			// Tạo đường dẫn lưu file
			var uploadPath = GetUploadPath(request.UserId);

			// KIỂM TRA WebRootPath
			if (string.IsNullOrEmpty(_env.WebRootPath))
			{
				throw new BusinessException(
					ErrorCode.ConfigurationError,
					"Web root path not configured");  
			}

			var fileName = $"{cvId}_{Guid.NewGuid()}.pdf";
			var filePath = Path.Combine(uploadPath, fileName);
			var relativePath = Path.Combine("uploads", "cvs",
				DateTime.UtcNow.ToString("yyyy"),
				DateTime.UtcNow.ToString("MM"),
				request.UserId.ToString(),
				fileName).Replace("\\", "/");

			// Tạo thư mục nếu chưa tồn tại
			Directory.CreateDirectory(uploadPath);

			// Lưu file
			try
			{
				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await request.FileStream.CopyToAsync(fileStream);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to save file to disk");
				throw new BusinessException(
					ErrorCode.FileUploadFailed,
					_msg.Business("FileUploadFailed"));  
			}

			// Lưu database
			var cv = new CV
			{
				Id = cvId,
				UserId = request.UserId,
				FileName = request.FileName,
				StoredFileName = fileName,
				FilePath = relativePath,
				FileSize = request.FileSize,
				ContentType = request.ContentType,
				Status = CVStatus.Processing,
				UploadedAt = DateTime.UtcNow
			};

			try
			{
				await _uow.CVs.AddAsync(cv);
				await _uow.SaveChangesAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to save CV to database");

				// Xóa file đã upload nếu lưu database thất bại
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}

				throw new BusinessException(
					ErrorCode.DatabaseError,
					_msg.Business("DatabaseError"));  
			}


			try
			{
				var extractedText = await _pdfService.ExtractTextAsync(filePath);
				cv.ExtractedText = extractedText;
				cv.Status = CVStatus.Completed;

				cv.ProcessedAt = DateTime.UtcNow;  
				cv.ErrorMessage = null;

				await _uow.SaveChangesAsync(cancellationToken);

				_logger.LogInformation("PDF text extracted successfully. Length: {Length}", extractedText.Length);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to extract text from PDF");
				cv.Status = CVStatus.Failed;
				cv.ErrorMessage = ex.Message;

				cv.ProcessedAt = DateTime.UtcNow;

				await _uow.SaveChangesAsync(cancellationToken);
			}

			return new UploadCVResponseDto
			{
				CvId = cvId,
				FileName = request.FileName,
				FilePath = relativePath,
				FileSize = request.FileSize,
				UploadedAt = cv.UploadedAt,
				Status = cv.Status.ToString()
			};
		}
		catch (BusinessException)
		{
			// Ném lại BusinessException để Controller xử lý
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error uploading CV for user {UserId}", request.UserId);
			throw new BusinessException(
				ErrorCode.InternalServerError,
				_msg.Business("InternalServerError"));  
		}
	}

	private string GetUploadPath(Guid userId)
	{
		var now = DateTime.UtcNow;

		if (string.IsNullOrEmpty(_env.WebRootPath))
		{
			throw new BusinessException(
				ErrorCode.ConfigurationError,
				"Web root path not configured");  
		}

		return Path.Combine(
			_env.WebRootPath,
			"uploads",
			"cvs",
			now.ToString("yyyy"),
			now.ToString("MM"),
			userId.ToString()
		);
	}
}