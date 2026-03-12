using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.API.Extensions;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecruitAI_API.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly ILogger<AuthController> _logger;
		private readonly IMessageService _msg;
		private readonly IWorkContext _workContext;

		public AuthController(
			IAuthService authService,
			ILogger<AuthController> logger,
			IMessageService messageService,
			IWorkContext workContext)
		{
			_authService = authService;
			_logger = logger;
			_msg = messageService;
			_workContext = workContext;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(
			[FromBody] RegisterRequestDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				var ipAddress = HttpContext.GetClientIpAddress();
				var result = await _authService.Register(request, ipAddress, cancellationToken);
				_logger.LogInformation(_msg.Log("RegistrationSuccess"), request.Email);
				return Ok(result);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Registration cancelled for email {Email}", request.Email);
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
				_logger.LogError(ex, _msg.Log("RegistrationError"), request.Email);
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

		[HttpPost("login")]
		public async Task<IActionResult> Login(
			[FromBody] LoginRequestDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				var ipAddress = HttpContext.GetClientIpAddress();
				var result = await _authService.Login(request, ipAddress, cancellationToken);
				_logger.LogInformation(_msg.Log("LoginSuccess"), request.Email);
				return Ok(result);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Login cancelled for email {Email}", request.Email);
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
				_logger.LogError(ex, _msg.Log("LoginError"), request.Email);
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

		[HttpPost("logout")]
		public async Task<IActionResult> Logout(
			LogoutRequestDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				var ipAddress = HttpContext.GetClientIpAddress();
				await _authService.Logout(request.RefreshToken, ipAddress, cancellationToken);
				return Ok(new { message = "Logged out successfully" });
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Logout cancelled");
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
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in logout");
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

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken(
			RefreshTokenRequestDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				var ipAddress = HttpContext.GetClientIpAddress();
				var result = await _authService.RefreshToken(request.RefreshToken, ipAddress, cancellationToken);
				return Ok(result);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Refresh token cancelled");
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
				_logger.LogError(ex, _msg.Log("RefreshTokenError"));
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

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
		{
			try
			{
				var userId = User.GetUserId();
				if (userId == null)
				{
					var response = new ErrorResponseDto
					{
						StatusCode = 401,
						ErrorCode = ErrorCode.Unauthorized,
						Message = _msg.Business("CannotIdentifyUser"),
						TraceId = HttpContext.TraceIdentifier,
						Timestamp = DateTime.UtcNow
					};
					return Unauthorized(response);
				}

				var result = await _authService.GetCurrentUserAsync(userId.Value, cancellationToken);
				return Ok(result);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Get current user cancelled");
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
				_logger.LogError(ex, "Error getting current user");
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

		[HttpPost("change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword(
			[FromBody] ChangePasswordRequestDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				var userId = _workContext.GetCurrentUserId();
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

				var result = await _authService.ChangePasswordAsync(request, userId.Value, cancellationToken);

				return Ok(result);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Change password cancelled");
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
				_logger.LogError(ex, "Error in change password endpoint");
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

		[HttpPost("forgot-password")]
		[AllowAnonymous]
		public async Task<IActionResult> ForgotPassword(
			ForgotPasswordRequestDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				var ipAddress = HttpContext.GetClientIpAddress();
				var result = await _authService.ForgotPasswordAsync(request, ipAddress, cancellationToken);
				return Ok(result);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Forgot password cancelled");
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
				_logger.LogError(ex, "Error in forgot password");
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

		[HttpPost("reset-password")]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword(
			ResetPasswordRequestDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				var ipAddress = HttpContext.GetClientIpAddress();
				var result = await _authService.ResetPasswordAsync(request, ipAddress, cancellationToken);
				return Ok(result);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Reset password cancelled");
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
				_logger.LogError(ex, "Error in reset password");
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
}