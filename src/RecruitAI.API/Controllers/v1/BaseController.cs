using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.DTOs.Responses.Auths;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Security.Claims;

namespace RecruitAI_API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
	protected readonly IMediator _mediator;
	protected readonly ILogger _logger;
	protected readonly IMessageService _msg;
	protected readonly IWorkContext? _workContext;

	protected BaseController(
		IMediator mediator,
		ILogger logger,
		IMessageService messageService)
	{
		_mediator = mediator;
		_logger = logger;
		_msg = messageService;
	}

	protected BaseController(
		IMediator mediator,
		ILogger logger,
		IMessageService messageService,
		IWorkContext workContext)
	{
		_mediator = mediator;
		_logger = logger;
		_msg = messageService;
		_workContext = workContext;
	}


	/// <summary>
	/// Lấy UserId từ Claims
	/// </summary>
	protected Guid? GetCurrentUserId()
	{
		// Ưu tiên 1: Claims
		var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (Guid.TryParse(userIdClaim, out var claimUserId))
		{
			return claimUserId;
		}

		// Ưu tiên 2: Work context (nếu có)
		if (_workContext != null)
		{
			var contextUserId = _workContext.GetCurrentUserId();
			if (contextUserId.HasValue)
			{
				return contextUserId;
			}
		}

		return null;
	}

	/// <summary>
	/// Xử lý request và trả về response với error handling đồng nhất - Trả về ActionResult<T>
	/// </summary>
	protected async Task<ActionResult<T>> ExecuteAsync<T>(
		Func<Task<T>> action,
		string successMessage = null)
	{
		try
		{
			var result = await action();

			if (!string.IsNullOrEmpty(successMessage))
			{
				_logger.LogInformation(_msg.Log(successMessage));
			}

			return Ok(result);
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Request cancelled");
			return StatusCode(499, CreateErrorResponse(
				499,
				ErrorCode.OperationCancelled,
				"RequestCancelled"));
		}
		catch (ValidationException ex)
		{
			return BadRequest(CreateValidationErrorResponse(ex));
		}
		catch (BusinessException ex)
		{
			return StatusCode(ex.StatusCode, CreateErrorResponse(
				ex.StatusCode,
				ex.ErrorCode,
				ex.Message));
		}
		catch (UnauthorizedAccessException)
		{
			return Unauthorized(CreateErrorResponse(
				401,
				ErrorCode.Unauthorized,
				_msg.Business("Unauthorized")));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error");
			return StatusCode(500, CreateErrorResponse(
				500,
				ErrorCode.InternalServerError,
				"InternalServerError"));
		}
	}

	/// <summary>
	/// Xử lý request và trả về response với error handling đồng nhất - Trả về IActionResult
	/// </summary>
	protected async Task<IActionResult> ExecuteAsync(
		Func<Task> action,
		string successMessage = null)
	{
		try
		{
			await action();

			if (!string.IsNullOrEmpty(successMessage))
			{
				_logger.LogInformation(_msg.Log(successMessage));
				return Ok(new { message = successMessage });
			}

			return Ok();
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Request cancelled");
			return StatusCode(499, CreateErrorResponse(
				499,
				ErrorCode.OperationCancelled,
				"RequestCancelled"));
		}
		catch (ValidationException ex)
		{
			return BadRequest(CreateValidationErrorResponse(ex));
		}
		catch (BusinessException ex)
		{
			return StatusCode(ex.StatusCode, CreateErrorResponse(
				ex.StatusCode,
				ex.ErrorCode,
				ex.Message));
		}
		catch (UnauthorizedAccessException)
		{
			return Unauthorized(CreateErrorResponse(
				401,
				ErrorCode.Unauthorized,
				"Unauthorized"));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error");
			return StatusCode(500, CreateErrorResponse(
				500,
				ErrorCode.InternalServerError,
				"InternalServerError"));
		}
	}

	/// <summary>
	/// Tạo ErrorResponseDto
	/// </summary>
	protected ErrorResponseDto CreateErrorResponse(
		int statusCode,
		ErrorCode errorCode,
		string message)
	{
		return new ErrorResponseDto
		{
			StatusCode = statusCode,
			ErrorCode = errorCode,
			Message = message,
			TraceId = HttpContext.TraceIdentifier,
			Timestamp = DateTime.UtcNow
		};
	}

	/// <summary>
	/// Tạo Validation Error Response
	/// </summary>
	protected ErrorResponseDto CreateValidationErrorResponse(ValidationException ex)
	{
		var errors = ex.Errors
			.GroupBy(x => x.PropertyName)
			.ToDictionary(
				g => g.Key,
				g => g.Select(x => x.ErrorMessage).ToArray()
			);

		return new ErrorResponseDto
		{
			StatusCode = 400,
			ErrorCode = ErrorCode.ValidationFailed,
			Message = _msg.Validation("ValidationFailed"),
			Errors = errors,
			TraceId = HttpContext.TraceIdentifier,
			Timestamp = DateTime.UtcNow
		};
	}

	/// <summary>
	/// Lấy IP Address từ HttpContext
	/// </summary>
	protected string GetClientIpAddress()
	{
		return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
	}
}