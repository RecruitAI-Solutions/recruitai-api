using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using RecruitAI.Application.DTOs.Auths;
using RecruitAI.Application.DTOs.Requests.Auths;
using RecruitAI.Application.DTOs.Responses.Auths;

namespace RecruitAI_API.Controllers.v1;

[AllowAnonymous]
public class AuthController : BaseController
{
	private readonly IAuthService _authService;
	private readonly IRolePermissionService _rolePermissionService;

	public AuthController(
		IMediator mediator,
		ILogger<AuthController> logger,
		IMessageService messageService,
		IAuthService authService,
		IWorkContext workContext,
		IRolePermissionService rolePermissionService)
		: base(mediator, logger, messageService, workContext)
	{
		_authService = authService;
		_rolePermissionService = rolePermissionService;
	}

	#region Authentication Endpoints

	[HttpPost("register")]
	public async Task<ActionResult<AuthResponseDto>> Register(
		[FromBody] RegisterRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<AuthResponseDto>(async () =>
		{
			var ipAddress = GetClientIpAddress();
			var result = await _authService.Register(request, ipAddress, cancellationToken);
			return result;
		}, "RegistrationSuccess");
	}

	[HttpPost("login")]
	public async Task<ActionResult<AuthResponseDto>> Login(
		[FromBody] LoginRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<AuthResponseDto>(async () =>
		{
			var ipAddress = GetClientIpAddress();
			var result = await _authService.Login(request, ipAddress, cancellationToken);
			return result;
		}, "LoginSuccess");
	}

	[HttpPost("logout")]
	public async Task<IActionResult> Logout(
		LogoutRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync(async () =>
		{
			var ipAddress = GetClientIpAddress();
			await _authService.Logout(request.RefreshToken, ipAddress, cancellationToken);
		});
	}

	[HttpPost("refresh-token")]
	public async Task<ActionResult<AuthResponseDto>> RefreshToken(
		RefreshTokenRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<AuthResponseDto>(async () =>
		{
			var ipAddress = GetClientIpAddress();
			var result = await _authService.RefreshToken(request.RefreshToken, ipAddress, cancellationToken);
			return result;
		});
	}

	#endregion

	#region User Profile Endpoints

	[HttpGet("me")]
	[Authorize(Policy = "ViewProfile")]
	public async Task<ActionResult<UserProfileDto>> GetCurrentUser(CancellationToken cancellationToken)
	{
		return await ExecuteAsync<UserProfileDto>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var result = await _authService.GetCurrentUserAsync(userId.Value, cancellationToken);
			return result;
		});
	}

	[HttpPost("change-password")]
	[Authorize(Policy = "ChangePassword")]
	public async Task<ActionResult<ChangePasswordResponseDto>> ChangePassword(
		[FromBody] ChangePasswordRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<ChangePasswordResponseDto>(async () =>
		{
			var userId = _workContext.GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var result = await _authService.ChangePasswordAsync(request, userId.Value, cancellationToken);
			return result;
		});
	}

	#endregion

	#region Password Management Endpoints

	[HttpPost("forgot-password")]
	[AllowAnonymous]
	public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword(
		ForgotPasswordRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<ForgotPasswordResponseDto>(async () =>
		{
			var ipAddress = GetClientIpAddress();
			var result = await _authService.ForgotPasswordAsync(request, ipAddress, cancellationToken);
			return result;
		});
	}

	[HttpPost("reset-password")]
	[AllowAnonymous]
	public async Task<ActionResult<ResetPasswordResponseDto>> ResetPassword(
		ResetPasswordRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<ResetPasswordResponseDto>(async () =>
		{
			var ipAddress = GetClientIpAddress();
			var result = await _authService.ResetPasswordAsync(request, ipAddress, cancellationToken);
			return result;
		});
	}

	#endregion

	#region Email Verification Endpoints

	[HttpPost("send-verification-email")]
	[AllowAnonymous]
	public async Task<ActionResult<SendVerificationEmailResponseDto>> SendVerificationEmail(
		[FromBody] SendVerificationEmailRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<SendVerificationEmailResponseDto>(async () =>
		{
			var ipAddress = GetClientIpAddress();
			var result = await _authService.SendVerificationEmailAsync(request, ipAddress, cancellationToken);
			return result;
		});
	}

	[HttpPost("verify-email")]
	[AllowAnonymous]
	public async Task<ActionResult<VerifyEmailResponseDto>> VerifyEmail(
		[FromBody] VerifyEmailRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<VerifyEmailResponseDto>(async () =>
		{
			var result = await _authService.VerifyEmailAsync(request, cancellationToken);
			return result;
		});
	}

	#endregion

	#region Role & Permission Endpoints

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
	[Authorize(Policy = "ViewPermissions")]
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

	#endregion

	#region External Login Endpoints

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

		var ipAddress = GetClientIpAddress();
		var result = await _authService.ExternalLoginAsync(provider, providerKey, email, name, ipAddress);

		if (!string.IsNullOrEmpty(returnUrl))
		{
			return Redirect($"{returnUrl}?token={result.AccessToken}&refreshToken={result.RefreshToken}");
		}

		return Ok(result);
	}

	#endregion
}