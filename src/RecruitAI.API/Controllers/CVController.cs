using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.CVs;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

namespace RecruitAI.API.Controllers;

[Authorize]
public class CVController : BaseController
{
	private readonly IWebHostEnvironment _env;

	public CVController(
		IMediator mediator,
		ILogger<CVController> logger,
		IMessageService messageService,
		IWebHostEnvironment env)
		: base(mediator, logger, messageService)
	{
		_env = env;
	}

	[HttpPost("upload")]
	[RequestSizeLimit(10 * 1024 * 1024)]
	public async Task<ActionResult<UploadCVResponseDto>> UploadCV(IFormFile file)
	{
		return await ExecuteAsync<UploadCVResponseDto>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			if (file == null || file.Length == 0)
				throw new BusinessException(ErrorCode.InvalidFile, _msg.Business("NoFileUploaded"));

			if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
				throw new BusinessException(ErrorCode.InvalidFileType, _msg.Business("OnlyPdfAllowed"));

			_logger.LogInformation(_msg.Log("UploadingCV"), file.FileName, file.Length);

			using var stream = new MemoryStream();
			await file.CopyToAsync(stream);
			stream.Position = 0;

			var command = new UploadCVCommand
			{
				UserId = userId.Value,
				FileName = file.FileName,
				FileStream = stream,
				FileSize = file.Length,
				ContentType = file.ContentType
			};

			var result = await _mediator.Send(command);
			return result;
		}, "UploadCVSuccess");
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> DownloadCV(Guid id)
	{
		// Xác thực user
		var userId = GetCurrentUserId();
		if (userId == null)
		{
			var response = new ErrorResponseDto
			{
				StatusCode = 401,
				ErrorCode = ErrorCode.Unauthorized,
				Message = _msg.Business("UserNotAuthenticated"),
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return Unauthorized(response);
		}

		try
		{
			// Lấy thông tin CV
			var query = new GetCVByIdQuery { Id = id, UserId = userId.Value };
			var cv = await _mediator.Send(query);

			if (cv == null)
			{
				var response = new ErrorResponseDto
				{
					StatusCode = 404,
					ErrorCode = ErrorCode.CVNotFound,
					Message = _msg.Business("CVNotFound"),
					TraceId = HttpContext.TraceIdentifier,
					Timestamp = DateTime.UtcNow
				};
				return NotFound(response);
			}

			// Kiểm tra file tồn tại
			var filePath = Path.Combine(_env.WebRootPath, cv.FilePath);
			if (!System.IO.File.Exists(filePath))
			{
				var response = new ErrorResponseDto
				{
					StatusCode = 404,
					ErrorCode = ErrorCode.CVFileMissing,
					Message = _msg.Business("CVFileMissing"),
					TraceId = HttpContext.TraceIdentifier,
					Timestamp = DateTime.UtcNow
				};
				return NotFound(response);
			}

			// Trả về file
			var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
			return File(fileBytes, "application/pdf", cv.FileName);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, _msg.Log("DownloadCVError"), id);

			var response = new ErrorResponseDto
			{
				StatusCode = 500,
				ErrorCode = ErrorCode.InternalServerError,
				Message = _msg.Business("InternalServerError"),
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return StatusCode(500, response);
		}
	}

	[HttpGet("my-cvs")]
	public async Task<ActionResult<IEnumerable<CV>>> GetMyCVs()
	{
		return await ExecuteAsync<IEnumerable<CV>>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var query = new GetUserCVsQuery { UserId = userId.Value };
			var cvs = await _mediator.Send(query);
			return cvs;
		});
	}
}