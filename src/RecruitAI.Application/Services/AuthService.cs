using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;

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

		public AuthService(
			IUnitOfWork uow,
			IJwtService jwtService,
			ILogger<AuthService> logger,
			IConfiguration configuration,
			IMessageService messageService,
			IRefreshTokenService refreshTokenService,
			IValidationService validationService,
			IWorkContext workContext)
		{
			_uow = uow;
			_jwtService = jwtService;
			_logger = logger;
			_configuration = configuration;
			_msg = messageService;
			_refreshTokenService = refreshTokenService;
			_validationService = validationService;
			_workContext = workContext;
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
					_msg.Throw(ErrorCode.ValidationFailed, "PasswordTooWeak");

				// Kiểm tra role hợp lệ
				if (!_validationService.IsValidRole(request.Role.ToString()))
					_msg.Throw(ErrorCode.ValidationFailed, "InvalidRole");

				await _uow.BeginTransactionAsync(cancellationToken);

				// Tạo user mới
				var user = new User
				{
					Id = Guid.NewGuid(),
					Email = request.Email,
					FullName = request.FullName,
					CreatedAt = DateTime.UtcNow,
					Status = UserStatus.Active,
					Role = request.Role
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

				// Log thành công
				_logger.LogInformation(_msg.Log("RegistrationSuccess"), request.Email);

				return new AuthResponseDto
				{
					AccessToken = token,
					RefreshToken = refreshToken,
					UserId = user.Id.ToString(),
					Email = user.Email,
					FullName = user.FullName,
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
				if (user.Status != UserStatus.Active)
					_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");

				// Cập nhật thời gian đăng nhập
				await _uow.AuthProviders.UpdateLastLoginAsync(provider.Id, cancellationToken);
				await _uow.Users.UpdateLastLoginAsync(user.Id, cancellationToken);

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
					_logger.LogWarning($"Inactive user {user.Email} tried to refresh token");
					_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");
				}

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
					return new ChangePasswordResponseDto
					{
						Success = false,
						Message = _msg.Business("UserNotFound"),
						Timestamp = DateTime.UtcNow
					};
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
					return new ChangePasswordResponseDto
					{
						Success = false,
						Message = _msg.Business("NoLocalAuthProvider"),
						Timestamp = DateTime.UtcNow
					};
				}

				// Kiểm tra mật khẩu cũ
				if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, authProvider.PasswordHash))
				{
					_logger.LogWarning("Invalid current password for user: {UserId}", userId);
					return new ChangePasswordResponseDto
					{
						Success = false,
						Message = _msg.Business("InvalidCurrentPassword"),
						Timestamp = DateTime.UtcNow
					};
				}

				// Kiểm tra mật khẩu mới không giống mật khẩu cũ
				if (request.CurrentPassword == request.NewPassword)
				{
					return new ChangePasswordResponseDto
					{
						Success = false,
						Message = _msg.Business("NewPasswordSameAsOld"),
						Timestamp = DateTime.UtcNow
					};
				}

				// Kiểm tra độ mạnh của mật khẩu mới
				if (!_validationService.IsStrongPassword(request.NewPassword))
				{
					return new ChangePasswordResponseDto
					{
						Success = false,
						Message = _msg.Validation("PasswordTooWeak"),
						Timestamp = DateTime.UtcNow
					};
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
			catch (Exception ex)
			{
				await _uow.RollbackTransactionAsync(cancellationToken);
				_logger.LogError(ex, "Error changing password for user: {UserId}", userId);

				return new ChangePasswordResponseDto
				{
					Success = false,
					Message = _msg.Business("PasswordChangeFailed"),
					Timestamp = DateTime.UtcNow
				};
			}
		}

		public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null)
				_msg.Throw(ErrorCode.UserNotFound, "BusinessUserNotFound");

			return new UserProfileDto
			{
				UserId = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				Role = user.Role,
				Gender = user.Gender,
				PhoneNumber = user.PhoneNumber,
				DateOfBirth = user.DateOfBirth,
				Status = user.Status,
				CreatedAt = user.CreatedAt,
				LastLoginAt = user.LastLoginAt,
				AvatarUrl = user.AvatarUrl
			};
		}
	}
}