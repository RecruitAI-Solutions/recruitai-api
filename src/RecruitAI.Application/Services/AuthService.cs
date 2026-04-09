using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Auths;
using RecruitAI.Application.DTOs.Requests.Auths;
using RecruitAI.Application.DTOs.Responses.Auths;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text.Json;

namespace RecruitAI.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly IUnitOfWork _uow;
		private readonly IJwtService _jwtService;
		private readonly ILogger<AuthService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IMessageService _msg;
		private readonly IRefreshTokenService _refreshTokenService;
		private readonly IValidationService _validationService;
		private readonly IWorkContext _workContext;
		private readonly IEmailService _emailService;
		private readonly IRolePermissionService _rolePermissionService;
		private readonly IAuditLogService _auditLogService;

		public AuthService(
			IUnitOfWork uow,
			IJwtService jwtService,
			ILogger<AuthService> logger,
			IConfiguration configuration,
			IMessageService messageService,
			IRefreshTokenService refreshTokenService,
			IValidationService validationService,
			IWorkContext workContext,
			IEmailService emailService,
			IRolePermissionService rolePermissionService,
			IAuditLogService auditLogService)
		{
			_uow = uow;
			_jwtService = jwtService;
			_logger = logger;
			_configuration = configuration;
			_msg = messageService;
			_refreshTokenService = refreshTokenService;
			_validationService = validationService;
			_workContext = workContext;
			_emailService = emailService;
			_rolePermissionService = rolePermissionService;
			_auditLogService = auditLogService;
		}

		public async Task<AuthResponseDto> Register(RegisterRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
		{
			try
			{
				// Kiểm tra email unique dùng ValidationService
				var isEmailUnique = await _validationService.IsEmailUniqueAsync(request.Email, cancellationToken);
				if (!isEmailUnique)
					_msg.Throw(ErrorCode.EmailAlreadyExists, "EmailExists");

				// Kiểm tra password mạnh
				if (!_validationService.IsStrongPassword(request.Password))
					_msg.Throw(ErrorCode.PasswordTooWeak, "PasswordTooWeak");

				// Kiểm tra role hợp lệ
				if (!_validationService.IsValidRole(request.Role.ToString()))
					_msg.Throw(ErrorCode.ValidationFailed, "InvalidRole");

				await _uow.BeginTransactionAsync(cancellationToken);

				var roleCode = request.Role.ToString();
				var permissions = _rolePermissionService.GetPermissionsForRole(roleCode);

				var user = new User
				{
					Id = Guid.NewGuid(),
					Email = request.Email,
					FullName = request.FullName,
					CreatedAt = DateTime.UtcNow,
					Status = UserStatus.PendingVerification,
					Role = request.Role,
					PermissionCodes = string.Join(",", permissions)
				};

				if (request.Gender != null)
					user.Gender = request.Gender;
				if (request.PhoneNumber != null)
					user.PhoneNumber = request.PhoneNumber;
				if (request.DateOfBirth != null)
					user.DateOfBirth = request.DateOfBirth;

				await _uow.Users.AddAsync(user, cancellationToken);

				var provider = new AuthProvider
				{
					Id = Guid.NewGuid(),
					Provider = AuthProviderType.Email,
					ProviderUserId = request.Email,
					PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
					UserId = user.Id,
					CreatedAt = DateTime.UtcNow,
					ProviderEmail = request.Email
				};

				await _uow.AuthProviders.AddAsync(provider, cancellationToken);

				var refreshToken = _refreshTokenService.GenerateToken();
				var tokenEntity = new RefreshToken
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					Token = refreshToken,
					ExpireAt = DateTime.UtcNow.AddDays(7),
					CreatedAt = DateTime.UtcNow,
					CreatedByIp = ipAddress,
					TokenType = TokenType.RefreshToken,
					IsRevoked = false
				};

				await _uow.RefreshTokens.AddAsync(tokenEntity, cancellationToken);

				// Ghi audit log: User Created
				await _auditLogService.LogAsync(
					AuditEntityType.User,
					AuditAction.Create,
					user.Id,
					user.Email,
					null,
					JsonSerializer.Serialize(new Dictionary<string, string>
					{
						[_msg.Get("AuditFieldEmail")] = user.Email,
						[_msg.Get("AuditFieldRole")] = user.Role.ToString()
					}),
					null,
					cancellationToken);

				await _uow.CommitTransactionAsync(cancellationToken);

				var token = await _jwtService.GenerateToken(user);
				var expiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
				var expirySeconds = expiryMinutes * 60;
				var roleDef = _rolePermissionService.GetRoleDefinition(roleCode);			

				_logger.LogInformation(_msg.Log("RegistrationSuccess"), request.Email);

				return new AuthResponseDto
				{
					AccessToken = token,
					RefreshToken = refreshToken,
					UserId = user.Id.ToString(),
					Email = user.Email,
					FullName = user.FullName,
					Role = (int)user.Role,
					RoleName = roleDef?.Name ?? roleCode,
					Permissions = permissions,
					ExpiresIn = expirySeconds
				};
			}
			catch (OperationCanceledException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogWarning("Registration cancelled for email {Email}", request.Email);
				throw;
			}
			catch (BusinessException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				throw;
			}
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, _msg.Log("RegistrationError"), request.Email);
				_msg.Throw(ErrorCode.InternalServerError, "RegistrationFailed", ex, ex.Message);
				return null;
			}
		}

		public async Task<AuthResponseDto> Login(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
		{
			try
			{
				var provider = await _uow.AuthProviders.GetLocalAuthByEmailAsync(request.Email, cancellationToken);
				if (provider == null)
					_msg.Throw(ErrorCode.InvalidCredentials, "InvalidCredentials");

				if (!BCrypt.Net.BCrypt.Verify(request.Password, provider.PasswordHash))
					_msg.Throw(ErrorCode.InvalidCredentials, "InvalidCredentials");

				var user = provider.User;

				switch (user.Status)
				{
					case UserStatus.Active:
						break;
					case UserStatus.Banned:
						_msg.Throw(ErrorCode.AccountBanned, "AccountBanned");
						break;
					case UserStatus.Locked:
						_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
						break;
					case UserStatus.Inactive:
					case UserStatus.PendingVerification:
						_msg.Throw(ErrorCode.AccountNotVerified, "AccountNotVerified");
						break;
					case UserStatus.Deleted:
						_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");
						break;
					default:
						_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
						break;
				}

				await _uow.AuthProviders.UpdateLastLoginAsync(provider.Id, cancellationToken);
				await _uow.Users.UpdateLastLoginAsync(user.Id, cancellationToken);

				var roleCode = user.Role.ToString().ToUpper();
				var permissions = user.GetPermissionList();
				var roleDef = _rolePermissionService.GetRoleDefinition(roleCode);

				var accessTokenExpiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
				var refreshTokenExpiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

				var refreshToken = _refreshTokenService.GenerateToken();

				await _uow.BeginTransactionAsync(cancellationToken);
				await _uow.RefreshTokens.RevokeAllUserTokensAsync(user.Id, ipAddress, refreshToken, cancellationToken);

				var tokenEntity = new RefreshToken
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					Token = refreshToken,
					ExpireAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
					CreatedAt = DateTime.UtcNow,
					TokenType = TokenType.RefreshToken,
					CreatedByIp = ipAddress,
					IsRevoked = false
				};

				// Ghi audit log: User Login
				await _auditLogService.LogAsync(
					AuditEntityType.User,
					AuditAction.Login,
					user.Id,
					user.Email,
					null,
					null,
					null,
					cancellationToken);

				await _uow.RefreshTokens.AddAsync(tokenEntity, cancellationToken);
				await _uow.CommitTransactionAsync(cancellationToken);

				var token = await _jwtService.GenerateToken(user);
				var expirySeconds = accessTokenExpiryMinutes * 60;

				_logger.LogInformation(_msg.Log("LoginSuccess"), request.Email);

				return new AuthResponseDto
				{
					AccessToken = token,
					RefreshToken = refreshToken,
					UserId = user.Id.ToString(),
					Email = user.Email,
					FullName = user.FullName,
					Role = (int)user.Role,
					RoleName = roleDef?.Name ?? roleCode,
					Permissions = permissions,
					ExpiresIn = expirySeconds
				};
			}
			catch (OperationCanceledException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogWarning("Login cancelled for email {Email}", request.Email);
				throw;
			}
			catch (BusinessException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				throw;
			}
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, _msg.Log("LoginError"), request.Email);
				_msg.Throw(ErrorCode.InternalServerError, "LoginFailed", ex, ex.Message);
				return null;
			}
		}

		public async Task Logout(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
		{
			try
			{
				var token = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken);

				if (token != null && !token.IsRevoked)
				{
					await _uow.RefreshTokens.RevokeTokenAsync(refreshToken, ipAddress, null, cancellationToken);

					if (token.User != null)
					{
						token.User.LastLoginAt = null;
						_uow.Users.Update(token.User);
					}

					// Ghi audit log: User Logout
					await _auditLogService.LogAsync(
						AuditEntityType.User,
						AuditAction.Logout,
						token.User.Id,
						token.User.Email,
						null,
						null,
						null,
						cancellationToken);

					await _uow.SaveChangesAsync(cancellationToken);

					_logger.LogInformation($"User {token.User?.Email} logged out from IP: {ipAddress}");
				}
				else
				{
					_logger.LogWarning($"Invalid logout attempt with token: {refreshToken} from IP: {ipAddress}");
				}
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Logout cancelled");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error during logout: {refreshToken}");
			}
		}

		public async Task<AuthResponseDto> RefreshToken(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
		{
			try
			{
				var token = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken);

				if (token == null || token.IsRevoked || token.IsExpired)
				{
					_logger.LogWarning($"Invalid refresh token attempt from IP: {ipAddress}");
					_msg.Throw(ErrorCode.InvalidToken, "InvalidRefreshToken");
				}

				var user = token.User;

				if (user.Status != UserStatus.Active)
				{
					if (user.Status == UserStatus.Banned)
						_msg.Throw(ErrorCode.AccountBanned, "AccountBanned");
					else if (user.Status == UserStatus.Locked)
						_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
					else if (user.Status == UserStatus.Inactive || user.Status == UserStatus.PendingVerification)
						_msg.Throw(ErrorCode.AccountNotVerified, "AccountNotVerified");
					else
						_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
				}

				var roleCode = user.Role.ToString().ToUpper();
				var permissions = user.GetPermissionList();
				var roleDef = _rolePermissionService.GetRoleDefinition(roleCode);

				var accessTokenExpiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
				var refreshTokenExpiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

				await _uow.BeginTransactionAsync(cancellationToken);

				var newRefreshToken = _refreshTokenService.GenerateToken();
				await _uow.RefreshTokens.RevokeTokenAsync(refreshToken, ipAddress, newRefreshToken, cancellationToken);

				var newTokenEntity = new RefreshToken
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					Token = newRefreshToken,
					ExpireAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
					CreatedAt = DateTime.UtcNow,
					CreatedByIp = ipAddress,
					TokenType = TokenType.RefreshToken,
					IsRevoked = false
				};

				await _auditLogService.LogAsync(
					AuditEntityType.User,
					AuditAction.RefreshToken,
					user.Id,
					user.Email,
					null,
					null,
					null,
					cancellationToken);
				await _uow.RefreshTokens.AddAsync(newTokenEntity, cancellationToken);
				await _uow.CommitTransactionAsync(cancellationToken);

				var accessToken = await _jwtService.GenerateToken(user);
				var expirySeconds = accessTokenExpiryMinutes * 60;

				_logger.LogInformation($"Token refreshed for user: {user.Email} from IP: {ipAddress}");

				return new AuthResponseDto
				{
					AccessToken = accessToken,
					RefreshToken = newRefreshToken,
					UserId = user.Id.ToString(),
					Email = user.Email,
					FullName = user.FullName,
					Role = (int)user.Role,
					RoleName = roleDef?.Name ?? roleCode,
					Permissions = permissions,
					ExpiresIn = expirySeconds
				};
			}
			catch (OperationCanceledException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogWarning("Refresh token cancelled");
				throw;
			}
			catch (BusinessException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				throw;
			}
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, "Error refreshing token");
				_msg.Throw(ErrorCode.InternalServerError, "TokenRefreshFailed");
				return null;
			}
		}

		public async Task<ChangePasswordResponseDto> ChangePasswordAsync(
			ChangePasswordRequestDto request,
			Guid userId,
			CancellationToken cancellationToken = default)
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
					cancellationToken.ThrowIfCancellationRequested();

				var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
				if (user == null)
				{
					_logger.LogWarning("User not found: {UserId}", userId);
					_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");
				}

				if (user.Status != UserStatus.Active)
				{
					if (user.Status == UserStatus.Banned)
						_msg.Throw(ErrorCode.AccountBanned, "AccountBanned");
					else if (user.Status == UserStatus.Locked)
						_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
					else
						_msg.Throw(ErrorCode.InvalidData, "Cannot change password");
				}

				var authProvider = await _uow.AuthProviders
					.FirstOrDefaultAsync(ap => ap.UserId == userId && ap.Provider == AuthProviderType.Email, cancellationToken);

				if (authProvider == null)
				{
					_logger.LogWarning("No local auth provider found for user: {UserId}", userId);
					_msg.Throw(ErrorCode.ValidationFailed, "NoLocalAuthProvider");
				}

				if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, authProvider.PasswordHash))
				{
					_logger.LogWarning("Invalid current password for user: {UserId}", userId);
					_msg.Throw(ErrorCode.InvalidCredentials, "InvalidCurrentPassword");
				}

				if (request.CurrentPassword == request.NewPassword)
				{
					_msg.Throw(ErrorCode.ValidationFailed, "NewPasswordSameAsOld");
				}

				if (!_validationService.IsStrongPassword(request.NewPassword))
				{
					_msg.Throw(ErrorCode.PasswordTooWeak, "PasswordTooWeak");
				}

				await _uow.BeginTransactionAsync(cancellationToken);

				authProvider.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
				_uow.AuthProviders.Update(authProvider);

				// Ghi audit log: User Change Password
				await _auditLogService.LogAsync(
					AuditEntityType.User,
					AuditAction.ChangePassword,
					user.Id,
					user.Email,
					null,
					null,
					null,
					cancellationToken);

				await _uow.RefreshTokens.RevokeAllUserTokensAsync(userId, _workContext.GetCurrentIpAddress() ?? "unknown", cancellationToken: cancellationToken);

				await _uow.CommitTransactionAsync(cancellationToken);				

				_logger.LogInformation("Password changed successfully for user: {UserId}", userId);

				return new ChangePasswordResponseDto
				{
					Success = true,
					Message = _msg.Success("PasswordChanged"),
					Timestamp = DateTime.UtcNow
				};
			}
			catch (OperationCanceledException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogWarning("Change password cancelled for user: {UserId}", userId);
				throw;
			}
			catch (BusinessException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				throw;
			}
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, "Error changing password for user: {UserId}", userId);
				_msg.Throw(ErrorCode.InternalServerError, "PasswordChangeFailed", ex, ex.Message);
				return null;
			}
		}

		public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null)
				_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");

			var roleCode = user.Role.ToString().ToUpper();
			var permissions = user.GetPermissionList();
			var roleDef = _rolePermissionService.GetRoleDefinition(roleCode);

			return new UserProfileDto
			{
				UserId = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				Role = (int)user.Role,
				RoleName = roleDef?.Name ?? roleCode,
				Permissions = permissions,
				Gender = user.Gender,
				PhoneNumber = user.PhoneNumber,
				DateOfBirth = user.DateOfBirth,
				Status = user.Status,
				CreatedAt = user.CreatedAt,
				LastLoginAt = user.LastLoginAt,
				AvatarUrl = user.AvatarUrl
			};
		}

		public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(
			ForgotPasswordRequestDto request,
			string ipAddress,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);

				if (user != null && (user.Status == UserStatus.Banned || user.Status == UserStatus.Locked))
				{
					_logger.LogWarning("Password reset requested for banned/locked account: {Email}", request.Email);
					return new ForgotPasswordResponseDto
					{
						Success = false,
						Message = _msg.Business("CannotResetPassword")
					};
				}

				if (user == null)
				{
					_logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
					return new ForgotPasswordResponseDto
					{
						Success = true,
						Message = _msg.Business("ResetPasswordEmailSent")
					};
				}

				await _uow.PasswordResetTokens.InvalidateAllUserTokensAsync(user.Id, cancellationToken);

				var bytes = new byte[48];
				using (var rng = RandomNumberGenerator.Create())
				{
					rng.GetBytes(bytes);
				}
				var token = Convert.ToBase64String(bytes)
					.Replace("/", "_")
					.Replace("+", "-")
					.Substring(0, 50);

				var resetToken = new PasswordResetToken
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					Token = token,
					ExpiryDate = DateTime.UtcNow.AddHours(24),
					CreatedByIp = ipAddress
				};

				await _uow.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
				await _uow.SaveChangesAsync(cancellationToken);

				var resetLink = $"{_configuration["App:ClientUrl"]}/reset-password?token={token}&email={user.Email}";
				await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink, user.FullName, cancellationToken);

				_logger.LogInformation("Password reset token generated for user: {UserId}", user.Id);

				return new ForgotPasswordResponseDto
				{
					Success = true,
					Message = _msg.Business("ResetPasswordEmailSent")
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in ForgotPassword for email: {Email}", request.Email);
				return new ForgotPasswordResponseDto
				{
					Success = false,
					Message = _msg.Business("ProcessingError")
				};
			}
		}

		public async Task<ResetPasswordResponseDto> ResetPasswordAsync(
			ResetPasswordRequestDto request,
			string ipAddress,
			CancellationToken cancellationToken = default)
		{
			await _uow.BeginTransactionAsync(cancellationToken);
			try
			{
				var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);

				if (user != null && (user.Status == UserStatus.Banned || user.Status == UserStatus.Locked))
				{
					_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
				}

				if (user == null)
				{
					_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");
				}

				var resetToken = await _uow.PasswordResetTokens.GetValidTokenAsync(request.Token, cancellationToken);
				if (resetToken == null || resetToken.UserId != user.Id)
				{
					_logger.LogWarning("Invalid or expired reset token attempt for user: {UserId}", user.Id);
					_msg.Throw(ErrorCode.InvalidToken, "InvalidResetToken");
				}

				if (!_validationService.IsStrongPassword(request.NewPassword))
				{
					_msg.Throw(ErrorCode.PasswordTooWeak, "PasswordTooWeak");
				}

				var authProvider = await _uow.AuthProviders
					.FirstOrDefaultAsync(ap => ap.UserId == user.Id && ap.Provider == AuthProviderType.Email, cancellationToken);

				if (authProvider == null)
				{
					_logger.LogError("User {UserId} has no local auth provider to reset password", user.Id);
					_msg.Throw(ErrorCode.ValidationFailed, "NoLocalAuthProvider");
				}

				authProvider.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
				_uow.AuthProviders.Update(authProvider);

				resetToken.IsUsed = true;
				resetToken.UsedAt = DateTime.UtcNow;
				_uow.PasswordResetTokens.Update(resetToken);

				// Ghi audit log: User Reset Password (ChangePassword action)
				await _auditLogService.LogAsync(
					AuditEntityType.User,
					AuditAction.ChangePassword,
					user.Id,
					user.Email,
					null,
					null,
					null,
					cancellationToken);

				await _uow.RefreshTokens.RevokeAllUserTokensAsync(user.Id, ipAddress, cancellationToken: cancellationToken);

				await _uow.CommitTransactionAsync(cancellationToken);

				_logger.LogInformation("Password reset successful for user: {UserId}", user.Id);

				return new ResetPasswordResponseDto
				{
					Success = true,
					Message = _msg.Success("PasswordReset"),
					Timestamp = DateTime.UtcNow
				};
			}
			catch (OperationCanceledException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogWarning("Reset password cancelled for email: {Email}", request.Email);
				throw;
			}
			catch (BusinessException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				throw;
			}
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, "Error in ResetPassword for email: {Email}", request.Email);
				_msg.Throw(ErrorCode.InternalServerError, "PasswordResetFailed", ex, ex.Message);
				return null;
			}
		}

		public async Task<SendVerificationEmailResponseDto> SendVerificationEmailAsync(
		SendVerificationEmailRequestDto request,
		string ipAddress,
		CancellationToken cancellationToken = default)
		{
			try
			{
				var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);

				if (user == null)
				{
					_logger.LogInformation("Verification email requested for non-existent email: {Email}", request.Email);
					return new SendVerificationEmailResponseDto
					{
						Success = true,
						Message = _msg.Business("VerificationEmailSent")
					};
				}

				if (user.EmailVerified)
				{
					_msg.Throw(ErrorCode.EmailAlreadyVerified, "EmailAlreadyVerified");
				}

				var bytes = new byte[48];
				using (var rng = RandomNumberGenerator.Create())
				{
					rng.GetBytes(bytes);
				}
				var token = Convert.ToBase64String(bytes)
					.Replace("/", "_")
					.Replace("+", "-")
					.Substring(0, 50);

				var verificationToken = new PasswordResetToken
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					Token = token,
					ExpiryDate = DateTime.UtcNow.AddHours(24),
					CreatedByIp = ipAddress,
					IsUsed = false
				};

				await _uow.PasswordResetTokens.AddAsync(verificationToken, cancellationToken);
				await _uow.SaveChangesAsync(cancellationToken);

				var verificationLink = $"{_configuration["App:ClientUrl"]}/verify-email?token={token}&email={user.Email}";
				await _emailService.SendVerificationEmailAsync(user.Email, verificationLink, user.FullName, cancellationToken);

				_logger.LogInformation("Verification email sent to user: {UserId}", user.Id);

				return new SendVerificationEmailResponseDto
				{
					Success = true,
					Message = _msg.Business("VerificationEmailSent")
				};
			}
			catch (BusinessException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending verification email to: {Email}", request.Email);
				_msg.Throw(ErrorCode.InternalServerError, "EmailServiceError", ex, ex.Message);
				return null;
			}
		}

		public async Task<VerifyEmailResponseDto> VerifyEmailAsync(
		VerifyEmailRequestDto request,
		CancellationToken cancellationToken = default)
		{
			await _uow.BeginTransactionAsync(cancellationToken);
			try
			{
				var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);
				if (user == null)
				{
					_logger.LogWarning("Verify email attempt for non-existent email: {Email}", request.Email);
					_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");
				}

				if (user.EmailVerified)
				{
					_logger.LogInformation("Email already verified for user: {UserId}", user.Id);
					return new VerifyEmailResponseDto
					{
						Success = true,
						Message = _msg.Success("EmailAlreadyVerified"),
						Timestamp = DateTime.UtcNow
					};
				}

				var token = await _uow.PasswordResetTokens.GetValidTokenAsync(request.Token, cancellationToken);
				if (token == null || token.UserId != user.Id)
				{
					_logger.LogWarning("Invalid or expired verification token for user: {UserId}", user.Id);
					_msg.Throw(ErrorCode.InvalidToken, "InvalidToken");
				}

				user.EmailVerified = true;

				if (user.Status == UserStatus.PendingVerification || user.Status == UserStatus.Inactive)
				{
					user.Status = UserStatus.Active;
				}

				_uow.Users.Update(user);

				token.IsUsed = true;
				token.UsedAt = DateTime.UtcNow;
				_uow.PasswordResetTokens.Update(token);

				// Ghi audit log: Email Verified (Update action)
				await _auditLogService.LogAsync(
					AuditEntityType.User,
					AuditAction.Update,
					user.Id,
					user.Email,
					"EmailVerified: false",
					"EmailVerified: true",
					"Email verification completed",
					cancellationToken);

				await _uow.CommitTransactionAsync(cancellationToken);		

				_logger.LogInformation("Email verified successfully for user: {UserId}", user.Id);

				return new VerifyEmailResponseDto
				{
					Success = true,
					Message = _msg.Success("EmailVerified"),
					Timestamp = DateTime.UtcNow
				};
			}
			catch (OperationCanceledException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogWarning("Verify email cancelled");
				throw;
			}
			catch (BusinessException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				throw;
			}
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, "Error verifying email for: {Email}", request.Email);
				_msg.Throw(ErrorCode.InternalServerError, "ProcessingError", ex, ex.Message);
				return null;
			}
		}

		public async Task<AuthResponseDto> ExternalLoginAsync(
		string provider,
		string providerUserId,
		string email,
		string name,
		string ipAddress,
		CancellationToken cancellationToken = default)
		{
			try
			{
				if (!Enum.TryParse<AuthProviderType>(provider, true, out var providerType))
				{
					_msg.Throw(ErrorCode.ValidationFailed, "InvalidProvider");
				}

				await _uow.BeginTransactionAsync(cancellationToken);

				var authProvider = await _uow.AuthProviders
					.GetByProviderAndUserIdAsync(providerType, providerUserId, cancellationToken);

				User user;

				if (authProvider == null)
				{
					user = await _uow.Users.GetByEmailAsync(email, cancellationToken);

					if (user != null && user.Status != UserStatus.Active)
					{
						if (user.Status == UserStatus.Banned)
							_msg.Throw(ErrorCode.AccountBanned, "AccountBanned");
						else if (user.Status == UserStatus.Locked)
							_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
						else
							_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
					}

					if (user == null)
					{
						user = new User
						{
							Id = Guid.NewGuid(),
							Email = email,
							FullName = name ?? email.Split('@')[0],
							CreatedAt = DateTime.UtcNow,
							Status = UserStatus.Active,
							Role = UserRole.CANDIDATE,
							EmailVerified = true
						};
						await _uow.Users.AddAsync(user, cancellationToken);

						// Ghi audit log: External User Created
						await _auditLogService.LogAsync(
							AuditEntityType.User,
							AuditAction.Create,
							user.Id,
							user.Email,
							null,
							JsonSerializer.Serialize(new Dictionary<string, string>
							{
								[_msg.Get("AuditFieldEmail")] = user.Email,
								[_msg.Get("AuditFieldProvider")] = provider
							}),
							$"User created via {provider}",
							cancellationToken);

						await _uow.SaveChangesAsync(cancellationToken);

						var newRoleCode = user.Role.ToString().ToUpper();
						var newPermissions = _rolePermissionService.GetPermissionsForRole(newRoleCode);
						user.SetPermissions(newPermissions);
					}

					authProvider = new AuthProvider
					{
						Id = Guid.NewGuid(),
						UserId = user.Id,
						Provider = providerType,
						ProviderUserId = providerUserId,
						ProviderEmail = email,
						CreatedAt = DateTime.UtcNow,
						LastLoginAt = DateTime.UtcNow
					};
					await _uow.AuthProviders.AddAsync(authProvider, cancellationToken);
				}
				else
				{
					user = authProvider.User;
					authProvider.LastLoginAt = DateTime.UtcNow;
					authProvider.ProviderEmail = email;
					_uow.AuthProviders.Update(authProvider);
				}

				user.LastLoginAt = DateTime.UtcNow;
				_uow.Users.Update(user);

				var refreshToken = _refreshTokenService.GenerateToken();
				var refreshTokenExpiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

				await _uow.RefreshTokens.RevokeAllUserTokensAsync(user.Id, ipAddress, refreshToken, cancellationToken);

				var tokenEntity = new RefreshToken
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					Token = refreshToken,
					ExpireAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
					CreatedAt = DateTime.UtcNow,
					CreatedByIp = ipAddress,
					TokenType = TokenType.RefreshToken,
					IsRevoked = false
				};

				// Ghi audit log: External User Login
				await _auditLogService.LogAsync(
					AuditEntityType.User,
					AuditAction.Login,
					user.Id,
					user.Email,
					null,
					null,
					$"Login via {provider}",
					cancellationToken);

				await _uow.RefreshTokens.AddAsync(tokenEntity, cancellationToken);

				await _uow.CommitTransactionAsync(cancellationToken);

				var accessToken = await _jwtService.GenerateToken(user);
				var accessTokenExpiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
				var expirySeconds = accessTokenExpiryMinutes * 60;

				var roleCode = user.Role.ToString().ToUpper();
				var roleDef = _rolePermissionService.GetRoleDefinition(roleCode);
				var permissions = user.GetPermissionList();

				_logger.LogInformation("User {Email} logged in via {Provider}", user.Email, provider);

				return new AuthResponseDto
				{
					AccessToken = accessToken,
					RefreshToken = refreshToken,
					UserId = user.Id.ToString(),
					Email = user.Email,
					FullName = user.FullName,
					Role = (int)user.Role,
					RoleName = roleDef?.Name ?? user.Role.ToString(),
					Permissions = permissions,
					ExpiresIn = expirySeconds,
					TokenType = "Bearer"
				};
			}
			catch (BusinessException)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				throw;
			}
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, "Error in external login");
				_msg.Throw(ErrorCode.InternalServerError, "SocialLoginFailed", ex, ex.Message);
				return null;
			}
		}
	}
}