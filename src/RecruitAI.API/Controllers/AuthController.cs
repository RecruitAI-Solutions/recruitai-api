using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
		private readonly IRolePermissionService _rolePermissionService;

		public AuthController(
			IAuthService authService,
			ILogger<AuthController> logger,
			IMessageService messageService,
			IWorkContext workContext,
			IRolePermissionService rolePermissionService)
		{
			_authService = authService;
			_logger = logger;
			_msg = messageService;
			_workContext = workContext;
			_rolePermissionService = rolePermissionService;
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

		[HttpPost("send-verification-email")]
		[AllowAnonymous]
		public async Task<IActionResult> SendVerificationEmail(
		[FromBody] SendVerificationEmailRequestDto request,
		CancellationToken cancellationToken)
		{
			try
			{
				var ipAddress = HttpContext.GetClientIpAddress();
				var result = await _authService.SendVerificationEmailAsync(request, ipAddress, cancellationToken);
				return Ok(result);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Send verification email cancelled");
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
				_logger.LogError(ex, "Error sending verification email");
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

		[HttpPost("verify-email")]
		[AllowAnonymous]
		public async Task<IActionResult> VerifyEmail(
		[FromBody] VerifyEmailRequestDto request,
		CancellationToken cancellationToken)
		{
			try
			{
				var result = await _authService.VerifyEmailAsync(request, cancellationToken);
				return Ok(result);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Verify email cancelled");
				var response = new ErrorResponseDto
				{
					StatusCode = 499,
					ErrorCode = ErrorCode.OperationCancelled,
					Message = "Request cancelled",
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
				_logger.LogError(ex, "Error verifying email");
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

		[HttpGet("config")]
		[AllowAnonymous] // Hoặc [Authorize(Roles = "ADMIN")]
		public async Task<IActionResult> GetConfig(CancellationToken cancellationToken, [FromQuery] string language = "vi")
		{
			var config = await _rolePermissionService.GetRoleConfigAsync(cancellationToken);

			// Transform for UI based on language
			var result = new
			{
				roles = config.Roles.Select(r => new
				{
					code = r.Code,
					name = language == "vi" ? r.NameVi : r.Name,
					permissions = r.Permissions
				}),
				permissions = config.Permissions.Select(p => new
				{
					code = p.Code,
					name = language == "vi" ? p.NameVi : p.Name,
					group = language == "vi" ? p.GroupVi : p.Group,
					description = language == "vi" ? p.DescriptionVi : p.Description
				})
			};

			return Ok(result);
		}

		[HttpGet("user-permissions")]
		[Authorize]
		public IActionResult GetUserPermissions([FromQuery] string language = "vi")
		{
			// Lấy roles từ claims - thử nhiều cách
			var roles = User.Claims
				.Where(c => c.Type == "role" ||
							c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
							c.Type == ClaimTypes.Role)
				.Select(c => c.Value)
				.ToList();

			if (!roles.Any())
			{
				_logger.LogWarning("No role claims found!");
				return Ok(new
				{
					roles = new List<string>(),
					permissions = new List<string>(),
					groupedPermissions = new Dictionary<string, object>()
				});
			}

			var result = _rolePermissionService.GetUserPermissions(roles, language);
			return Ok(result);
		}

		[HttpGet("login/{provider}")]
		[AllowAnonymous]
		public IActionResult ExternalLogin(string provider, string returnUrl = null)
		{
			// Kiểm tra provider hợp lệ
			var providers = new[] { "Google", "Facebook", "GitHub" };
			if (!providers.Contains(provider))
			{
				return BadRequest(new { message = "Invalid provider" });
			}

			// Cấu hình redirect URL
			var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth",
				new { returnUrl }, Request.Scheme);

			// Gửi request xác thực đến provider
			var properties = new AuthenticationProperties
			{
				RedirectUri = redirectUrl,
				Items = { { "provider", provider } }
			};

			return Challenge(properties, provider);
		}

		[HttpGet("external-login-callback")]
		[AllowAnonymous]
		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
		{
			if (remoteError != null)
			{
				_logger.LogError("Error from external provider: {Error}", remoteError);
				return BadRequest(new { message = $"External provider error: {remoteError}" });
			}

			var authenticateResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

			if (!authenticateResult.Succeeded)
			{
				return BadRequest(new { message = "External authentication failed" });
			}

			var provider = authenticateResult.Properties.Items["provider"];
			var providerKey = authenticateResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
			var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email)
						?? authenticateResult.Principal.FindFirstValue("email");
			var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);

			_logger.LogInformation("Provider: {Provider}, Email: {Email}", provider, email);

			if (string.IsNullOrEmpty(providerKey))
			{
				return BadRequest(new { message = "Could not get user info from provider" });
			}

			// Xóa cookie tạm
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

			var ipAddress = HttpContext.GetClientIpAddress();
			var result = await _authService.ExternalLoginAsync(provider, providerKey, email, name, ipAddress);

			if (!string.IsNullOrEmpty(returnUrl))
			{
				return Redirect($"{returnUrl}?token={result.AccessToken}&refreshToken={result.RefreshToken}");
			}

			return Ok(result);
		}
	}
}