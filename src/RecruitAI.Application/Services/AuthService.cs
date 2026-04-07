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
			IRolePermissionService rolePermissionService)
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

				var roleCode = request.Role.ToString(); // "CANDIDATE", "RECRUITER", "ADMIN"
				var permissions = _rolePermissionService.GetPermissionsForRole(roleCode);

				// Tạo user mới
				var user = new User
				{
					Id = Guid.NewGuid(),
					Email = request.Email,
					FullName = request.FullName,
					CreatedAt = DateTime.UtcNow,
					Status = UserStatus.PendingVerification,
					Role = request.Role,
					PermissionCodes = string.Join(",", permissions) // Lưu permissions
				};

				// Gán các thuộc tính optional
				if (request.Gender != null)
					user.Gender = request.Gender;
				if (request.PhoneNumber != null)
					user.PhoneNumber = request.PhoneNumber;
				if (request.DateOfBirth != null)
					user.DateOfBirth = request.DateOfBirth;

				await _uow.Users.AddAsync(user, cancellationToken);

				// Tạo auth provider với password đã hash
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

				// Tạo refresh token với IP được truyền vào
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

				// Save tất cả
				await _uow.CommitTransactionAsync(cancellationToken);

				// Generate token
				var token = await _jwtService.GenerateToken(user);

				// Đọc expiry từ config
				var expiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
				var expirySeconds = expiryMinutes * 60;

				var roleDef = _rolePermissionService.GetRoleDefinition(roleCode);

				// Log thành công
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
				// Tìm auth provider theo email
				var provider = await _uow.AuthProviders.GetLocalAuthByEmailAsync(request.Email, cancellationToken);
				if (provider == null)
					_msg.Throw(ErrorCode.InvalidCredentials, "InvalidCredentials");

				// Kiểm tra password
				if (!BCrypt.Net.BCrypt.Verify(request.Password, provider.PasswordHash))
					_msg.Throw(ErrorCode.InvalidCredentials, "InvalidCredentials");

				var user = provider.User;

				// Kiểm tra trạng thái user
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

				// Cập nhật thời gian đăng nhập
				await _uow.AuthProviders.UpdateLastLoginAsync(provider.Id, cancellationToken);
				await _uow.Users.UpdateLastLoginAsync(user.Id, cancellationToken);

				var roleCode = user.Role.ToString().ToUpper();
				var permissions = user.GetPermissionList(); // Lấy từ user
				var roleDef = _rolePermissionService.GetRoleDefinition(roleCode);

				// Đọc expiry từ config
				var accessTokenExpiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
				var refreshTokenExpiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

				var refreshToken = _refreshTokenService.GenerateToken();

				// Bắt đầu transaction
				await _uow.BeginTransactionAsync(cancellationToken);

				// Revoke tất cả refresh tokens cũ của user
				await _uow.RefreshTokens.RevokeAllUserTokensAsync(user.Id, ipAddress, refreshToken, cancellationToken);

				// Tạo refresh token mới
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

				await _uow.RefreshTokens.AddAsync(tokenEntity, cancellationToken);
				await _uow.CommitTransactionAsync(cancellationToken);

				// Generate token
				var token = await _jwtService.GenerateToken(user);
				var expirySeconds = accessTokenExpiryMinutes * 60;

				// Log thành công
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
				// Kiểm tra token có tồn tại không
				var token = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken);

				if (token != null && !token.IsRevoked)
				{
					// Revoke token
					await _uow.RefreshTokens.RevokeTokenAsync(refreshToken, ipAddress, null, cancellationToken);

					// Cập nhật thời gian logout (nếu muốn)
					if (token.User != null)
					{
						token.User.LastLoginAt = null;
						_uow.Users.Update(token.User);
					}

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
				// Tìm refresh token
				var token = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken);

				// Kiểm tra token hợp lệ
				if (token == null || token.IsRevoked || token.IsExpired)
				{
					_logger.LogWarning($"Invalid refresh token attempt from IP: {ipAddress}");
					_msg.Throw(ErrorCode.InvalidToken, "InvalidRefreshToken");
				}

				var user = token.User;

				// Kiểm tra user còn active không
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
				var permissions = user.GetPermissionList(); // Lấy từ user
				var roleDef = _rolePermissionService.GetRoleDefinition(roleCode);

				// Đọc expiry từ config
				var accessTokenExpiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
				var refreshTokenExpiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

				// Bắt đầu transaction
				await _uow.BeginTransactionAsync(cancellationToken);

				// Tạo refresh token MỚI
				var newRefreshToken = _refreshTokenService.GenerateToken();

				// Revoke token cũ
				await _uow.RefreshTokens.RevokeTokenAsync(refreshToken, ipAddress, newRefreshToken, cancellationToken);

				// Tạo token mới
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

				await _uow.RefreshTokens.AddAsync(newTokenEntity, cancellationToken);
				await _uow.CommitTransactionAsync(cancellationToken);

				// Generate access token mới
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
				// Kiểm tra cancellation
				if (cancellationToken.IsCancellationRequested)
					cancellationToken.ThrowIfCancellationRequested();

				// Lấy user từ database
				var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
				if (user == null)
				{
					_logger.LogWarning("User not found: {UserId}", userId);
					_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");
				}

				// Kiểm tra user có bị khóa/banned không
				if (user.Status != UserStatus.Active)
				{
					if (user.Status == UserStatus.Banned)
						_msg.Throw(ErrorCode.AccountBanned, "AccountBanned");
					else if (user.Status == UserStatus.Locked)
						_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
					else
						_msg.Throw(ErrorCode.InvalidData, "Cannot change password");
				}

				// Tìm auth provider local
				var authProvider = await _uow.AuthProviders
					.FirstOrDefaultAsync(ap =>
						ap.UserId == userId &&
						ap.Provider == AuthProviderType.Email,
						cancellationToken);

				if (authProvider == null)
				{
					_logger.LogWarning("No local auth provider found for user: {UserId}", userId);
					_msg.Throw(ErrorCode.ValidationFailed, "NoLocalAuthProvider");
				}

				// Kiểm tra mật khẩu cũ
				if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, authProvider.PasswordHash))
				{
					_logger.LogWarning("Invalid current password for user: {UserId}", userId);
					_msg.Throw(ErrorCode.InvalidCredentials, "InvalidCurrentPassword");
				}

				// Kiểm tra mật khẩu mới không giống mật khẩu cũ
				if (request.CurrentPassword == request.NewPassword)
				{
					_msg.Throw(ErrorCode.ValidationFailed, "NewPasswordSameAsOld");
				}

				// Kiểm tra độ mạnh của mật khẩu mới
				if (!_validationService.IsStrongPassword(request.NewPassword))
				{
					_msg.Throw(ErrorCode.PasswordTooWeak, "PasswordTooWeak");
				}

				// Bắt đầu transaction
				await _uow.BeginTransactionAsync(cancellationToken);

				// Hash mật khẩu mới
				authProvider.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
				_uow.AuthProviders.Update(authProvider);

				// Revoke tất cả refresh tokens (bắt buộc đăng nhập lại)
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
			var permissions = user.GetPermissionList(); // Lấy từ user
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
				// 1. Tìm user theo email (không phân biệt case)
				var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);

				if (user.Status == UserStatus.Banned || user.Status == UserStatus.Locked)
				{
					_logger.LogWarning("Password reset requested for banned/locked account: {Email}", request.Email);
					return new ForgotPasswordResponseDto
					{
						Success = false,
						Message = _msg.Business("CannotResetPassword")
					};
				}

				// 2. Luôn trả về thành công để tránh lộ thông tin email
				if (user == null)
				{
					_logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
					return new ForgotPasswordResponseDto
					{
						Success = true,
						Message = _msg.Business("ResetPasswordEmailSent")
					};
				}

				// 3. Vô hiệu hóa tất cả token cũ của user này
				await _uow.PasswordResetTokens.InvalidateAllUserTokensAsync(user.Id, cancellationToken);

				// 4. Tạo token mới
				var bytes = new byte[48]; // 48 bytes → Base64 ~ 64 ký tự
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

				// 5. Tạo link reset và gửi email
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
				// 1. Tìm user theo email
				var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);

				if (user.Status == UserStatus.Banned || user.Status == UserStatus.Locked)
				{
					_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
				}

				if (user == null)
				{
					_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");
				}

				// 2. Tìm token hợp lệ
				var resetToken = await _uow.PasswordResetTokens.GetValidTokenAsync(request.Token, cancellationToken);
				if (resetToken == null || resetToken.UserId != user.Id)
				{
					_logger.LogWarning("Invalid or expired reset token attempt for user: {UserId}", user.Id);
					_msg.Throw(ErrorCode.InvalidToken, "InvalidResetToken");
				}

				// 3. Kiểm tra password mạnh
				if (!_validationService.IsStrongPassword(request.NewPassword))
				{
					_msg.Throw(ErrorCode.PasswordTooWeak, "PasswordTooWeak");
				}

				// 4. Tìm AuthProvider local
				var authProvider = await _uow.AuthProviders
					.FirstOrDefaultAsync(ap => ap.UserId == user.Id && ap.Provider == AuthProviderType.Email, cancellationToken);

				if (authProvider == null)
				{
					_logger.LogError("User {UserId} has no local auth provider to reset password", user.Id);
					_msg.Throw(ErrorCode.ValidationFailed, "NoLocalAuthProvider");
				}

				// 5. Cập nhật mật khẩu mới
				authProvider.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
				_uow.AuthProviders.Update(authProvider);

				// 6. Vô hiệu hóa token vừa dùng
				resetToken.IsUsed = true;
				resetToken.UsedAt = DateTime.UtcNow;
				_uow.PasswordResetTokens.Update(resetToken);

				// 7. Revoke tất cả refresh tokens
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
				// 1. Tìm user theo email
				var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);

				if (user == null)
				{
					_logger.LogInformation("Verification email requested for non-existent email: {Email}", request.Email);
					return new SendVerificationEmailResponseDto
					{
						Success = true, // Luôn trả true để tránh lộ email
						Message = _msg.Business("VerificationEmailSent")
					};
				}

				// 2. Kiểm tra email đã verified chưa
				if (user.EmailVerified)
				{
					_msg.Throw(ErrorCode.EmailAlreadyVerified, "EmailAlreadyVerified");
				}

				// 3. Tạo verification token (có thể dùng chung bảng PasswordResetToken hoặc tạo bảng riêng)
				var bytes = new byte[48];
				using (var rng = RandomNumberGenerator.Create())
				{
					rng.GetBytes(bytes);
				}
				var token = Convert.ToBase64String(bytes)
					.Replace("/", "_")
					.Replace("+", "-")
					.Substring(0, 50);

				// 4. Lưu token (nên có bảng riêng hoặc dùng chung với expiration khác)
				var verificationToken = new PasswordResetToken // Tạm dùng chung bảng
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

				// 5. Tạo link verification
				var verificationLink = $"{_configuration["App:ClientUrl"]}/verify-email?token={token}&email={user.Email}";

				// 6. Gửi email
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
				// 1. Tìm user theo email
				var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);
				if (user == null)
				{
					_logger.LogWarning("Verify email attempt for non-existent email: {Email}", request.Email);
					_msg.Throw(ErrorCode.UserNotFound, "UserNotFound");
				}

				// 2. Kiểm tra email đã verified chưa
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

				// 3. Tìm token hợp lệ
				var token = await _uow.PasswordResetTokens.GetValidTokenAsync(request.Token, cancellationToken);
				if (token == null || token.UserId != user.Id)
				{
					_logger.LogWarning("Invalid or expired verification token for user: {UserId}", user.Id);
					_msg.Throw(ErrorCode.InvalidToken, "InvalidToken");
				}

				// 4. Cập nhật trạng thái verified
				user.EmailVerified = true;

				// Nếu status đang là PendingVerification hoặc Inactive, chuyển thành Active
				if (user.Status == UserStatus.PendingVerification || user.Status == UserStatus.Inactive)
				{
					user.Status = UserStatus.Active;
				}

				_uow.Users.Update(user);

				// 5. Vô hiệu hóa token
				token.IsUsed = true;
				token.UsedAt = DateTime.UtcNow;
				_uow.PasswordResetTokens.Update(token);

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

				// Tìm auth provider
				var authProvider = await _uow.AuthProviders
					.GetByProviderAndUserIdAsync(providerType, providerUserId, cancellationToken);

				User user;

				if (authProvider == null)
				{
					// Chưa từng đăng nhập bằng provider này
					user = await _uow.Users.GetByEmailAsync(email, cancellationToken);

					if (user.Status != UserStatus.Active)
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
						// Tạo user mới
						user = new User
						{
							Id = Guid.NewGuid(),
							Email = email,
							FullName = name ?? email.Split('@')[0],
							CreatedAt = DateTime.UtcNow,
							Status = UserStatus.Active,
							Role = UserRole.CANDIDATE, // Mặc định là Candidate
							EmailVerified = true // Email từ OAuth đã được xác thực
						};
						await _uow.Users.AddAsync(user, cancellationToken);

						await _uow.SaveChangesAsync(cancellationToken);

						// Set permissions từ config
						var newRoleCode = user.Role.ToString().ToUpper();
						var newPermissions = _rolePermissionService.GetPermissionsForRole(newRoleCode);
						user.SetPermissions(newPermissions);
					}

					// Tạo auth provider mới
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
					// Đã từng đăng nhập
					user = authProvider.User;
					authProvider.LastLoginAt = DateTime.UtcNow;
					authProvider.ProviderEmail = email;
					_uow.AuthProviders.Update(authProvider);
				}

				// Cập nhật last login
				user.LastLoginAt = DateTime.UtcNow;
				_uow.Users.Update(user);

				// Tạo refresh token
				var refreshToken = _refreshTokenService.GenerateToken();
				var refreshTokenExpiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

				// Revoke tất cả token cũ
				await _uow.RefreshTokens.RevokeAllUserTokensAsync(user.Id, ipAddress, refreshToken, cancellationToken);

				// Tạo token mới
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
				await _uow.RefreshTokens.AddAsync(tokenEntity, cancellationToken);

				await _uow.CommitTransactionAsync(cancellationToken);

				// Generate access token
				var accessToken = await _jwtService.GenerateToken(user);
				var accessTokenExpiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
				var expirySeconds = accessTokenExpiryMinutes * 60;

				// Lấy role definition
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