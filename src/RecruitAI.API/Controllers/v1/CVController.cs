using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.CVs;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.CVs;
using RecruitAI.Application.Commands.CVs;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using AutoMapper;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.DTOs.Responses.Auths;
using RecruitAI.Shared.DTOs;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI_API.Controllers.v1;
/// <summary>
/// Quản lý CV của ứng viên
/// </summary>
[Authorize]
[Route("api/v1/[controller]")]
public class CVController : BaseController
{
	private readonly IWebHostEnvironment _env;
	private readonly IMapper _mapper;
	private readonly IConfiguration _configuration;	

	public CVController(
		IMediator mediator,
		ILogger<CVController> logger,
		IMessageService messageService,
		IWebHostEnvironment env,
		IMapper mapper,
		IWorkContext workContext,
		IConfiguration configuration)
		: base(mediator, logger, messageService, workContext)
	{
		_env = env;
		_mapper = mapper;
		_configuration = configuration;
	}

	/// <summary>
	/// Tải lên CV mới (hỗ trợ file PDF)
	/// </summary>
	/// <param name="file">File PDF cần tải lên (tối đa 10MB)</param>
	/// <returns>Thông tin CV đã lưu</returns>
	[HttpPost("upload")]
	[Authorize(Policy = "UploadCV")]
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

	/// <summary>
	/// Lấy danh sách CV của tôi (ứng viên hiện tại)
	/// </summary>
	/// <param name="filter">Bộ lọc (trạng thái, từ khóa)</param>
	/// <param name="cancellationToken">Token hủy</param>
	/// <returns>Danh sách CV có phân trang</returns>
	[HttpGet("my-cvs")]
	[Authorize(Policy = "ViewOwnCVs")]
	[ProducesResponseType(typeof(PaginationResponseDto<CVListDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<PaginationResponseDto<CVListDto>>> GetMyCVs(
		[FromQuery] CVFilterDto filter,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<PaginationResponseDto<CVListDto>>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var query = new GetUserCVsQuery
			{
				UserId = userId.Value,
				Filter = filter
			};

			var result = await _mediator.Send(query, cancellationToken);
			return result;
		});
	}

	/// <summary>
	/// Lấy chi tiết CV theo ID
	/// </summary>
	/// <param name="id">ID của CV</param>
	/// <param name="cancellationToken">Token hủy</param>
	/// <returns>Thông tin chi tiết CV</returns>
	[HttpGet("{id}")]
	[Authorize(Policy = "DownloadOwnCV")]
	[ProducesResponseType(typeof(CVDetailDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<CVDetailDto>> GetCVDetail(
		Guid id,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<CVDetailDto>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var query = new GetCVByIdQuery { Id = id, UserId = userId.Value };
			var cv = await _mediator.Send(query, cancellationToken);

			return _mapper.Map<CVDetailDto>(cv);
		});
	}
	/// <summary>
	/// Tải file CV xuống máy
	/// </summary>
	/// <param name="id">ID của CV</param>
	/// <returns>File PDF</returns>
	[HttpGet("{id}/download")]
	[Authorize(Policy = "DownloadOwnCV")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> DownloadCV(Guid id)
	{
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

			var filePath = GetPhysicalFilePath(cv.FilePath);

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

	private string GetPhysicalFilePath(string relativePath)
	{
		var useSeparatePath = _configuration.GetValue<bool>("FileStorage:UseSeparateUploadPath", false);

		if (useSeparatePath)
		{
			var uploadRoot = _configuration["FileStorage:UploadRootPath"];
			if (string.IsNullOrEmpty(uploadRoot)) return null;
			return Path.Combine(uploadRoot, relativePath);
		}
		else
		{
			return Path.Combine(_env.WebRootPath, relativePath);
		}
	}

	/// <summary>
	/// Xóa CV (xóa mềm, có thể khôi phục)
	/// </summary>
	/// <param name="id">ID của CV</param>
	/// <param name="cancellationToken">Token hủy</param>
	/// <returns>Kết quả xóa</returns>
	[HttpDelete("{id}")]
	[Authorize(Policy = "DeleteOwnCV")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> DeleteCV(Guid id, CancellationToken cancellationToken)
	{
		return await ExecuteAsync(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var command = new DeleteCVCommand
			{
				Id = id,
				UserId = userId.Value
			};

			await _mediator.Send(command, cancellationToken);
		}, _msg.Business("CVDeleted"));
	}
}