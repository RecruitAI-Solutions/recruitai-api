using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.CVs;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.CVs;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Security.Claims;

namespace RecruitAI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CVController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly IWebHostEnvironment _env;
	private readonly ILogger<CVController> _logger;
	private readonly IMessageService _msg;

	public CVController(
		IMediator mediator,
		IWebHostEnvironment env,
		ILogger<CVController> logger,
		IMessageService messageService)
	{
		_mediator = mediator;
		_env = env;
		_logger = logger;
		_msg = messageService;
	}

	[HttpPost("upload")]
	[RequestSizeLimit(10 * 1024 * 1024)]
	public async Task<ActionResult<UploadCVResponse>> UploadCV(IFormFile file)
	{
		try
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
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

			if (file == null || file.Length == 0)
			{
				var response = new ErrorResponseDto
				{
					StatusCode = 400,
					ErrorCode = ErrorCode.InvalidRequest,
					Message = _msg.Business("NoFileUploaded"),
					TraceId = HttpContext.TraceIdentifier,
					Timestamp = DateTime.UtcNow
				};
				return BadRequest(response);
			}

			// Validate file type
			if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
			{
				var response = new ErrorResponseDto
				{
					StatusCode = 400,
					ErrorCode = ErrorCode.InvalidFileType,
					Message = _msg.Business("OnlyPdfAllowed"),
					TraceId = HttpContext.TraceIdentifier,
					Timestamp = DateTime.UtcNow
				};
				return BadRequest(response);
			}

			_logger.LogInformation(_msg.Log("UploadingCV"), file.FileName, file.Length);

			using var stream = new MemoryStream();
			await file.CopyToAsync(stream);
			stream.Position = 0;

			var command = new UploadCVCommand
			{
				UserId = Guid.Parse(userId),
				FileName = file.FileName,
				FileStream = stream,
				FileSize = file.Length,
				ContentType = file.ContentType
			};

			var result = await _mediator.Send(command);
			_logger.LogInformation(_msg.Log("UploadCVSuccess"), userId, result.CvId);

			return Ok(result);
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Upload CV cancelled");
			var response = new ErrorResponseDto
			{
				StatusCode = 499,
				ErrorCode = ErrorCode.OperationCancelled,
				Message = _msg.Business("RequestCancelled"),
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return StatusCode(499, response);
		}
		catch (ValidationException ex)
		{
			var errors = ex.Errors.GroupBy(x => x.PropertyName)
				.ToDictionary(
					g => g.Key,
					g => g.Select(x => x.ErrorMessage).ToArray()
				);

			var response = new ErrorResponseDto
			{
				StatusCode = 400,
				ErrorCode = ErrorCode.ValidationFailed,
				Message = _msg.Validation("ValidationFailed"), // "Dữ liệu nhập vào không hợp lệ"
				Errors = errors,
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return BadRequest(response);
		}
		catch (BusinessException ex)
		{
			var response = new ErrorResponseDto
			{
				StatusCode = ex.StatusCode,
				ErrorCode = ex.ErrorCode,
				Message = ex.Message,
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return StatusCode(ex.StatusCode, response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, _msg.Log("UploadCVError"));
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

	[HttpGet("{id}")]
	public async Task<IActionResult> DownloadCV(Guid id)
	{
		try
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
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

			var query = new GetCVByIdQuery { Id = id, UserId = Guid.Parse(userId) };
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

			var filePath = Path.Combine(_env.WebRootPath, cv.FilePath);
			if (!System.IO.File.Exists(filePath))
			{
				_logger.LogWarning(_msg.Log("CVFileMissing"), id);
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

			var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
			return File(fileBytes, "application/pdf", cv.FileName);
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Download CV cancelled");
			var response = new ErrorResponseDto
			{
				StatusCode = 499,
				ErrorCode = ErrorCode.OperationCancelled,
				Message = _msg.Business("RequestCancelled"),
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return StatusCode(499, response);
		}
		catch (UnauthorizedAccessException)
		{
			var response = new ErrorResponseDto
			{
				StatusCode = 403,
				ErrorCode = ErrorCode.Forbidden,
				Message = _msg.Business("AccessDenied"),
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return StatusCode(403, response);
		}
		catch (BusinessException ex)
		{
			var response = new ErrorResponseDto
			{
				StatusCode = ex.StatusCode,
				ErrorCode = ex.ErrorCode,
				Message = ex.Message,
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return StatusCode(ex.StatusCode, response);
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
		try
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
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

			var query = new GetUserCVsQuery { UserId = Guid.Parse(userId) };
			var cvs = await _mediator.Send(query);

			return Ok(cvs);
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Get my CVs cancelled");
			var response = new ErrorResponseDto
			{
				StatusCode = 499,
				ErrorCode = ErrorCode.OperationCancelled,
				Message = _msg.Business("RequestCancelled"),
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return StatusCode(499, response);
		}
		catch (BusinessException ex)
		{
			var response = new ErrorResponseDto
			{
				StatusCode = ex.StatusCode,
				ErrorCode = ex.ErrorCode,
				Message = ex.Message,
				TraceId = HttpContext.TraceIdentifier,
				Timestamp = DateTime.UtcNow
			};
			return StatusCode(ex.StatusCode, response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, _msg.Log("GetMyCVsError"));
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
}