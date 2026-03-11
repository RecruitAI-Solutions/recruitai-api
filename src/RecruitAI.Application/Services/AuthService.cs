using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly RecruitDevContext _context;
		private readonly IJwtService _jwtService;
		private readonly ILogger<AuthService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IMessageService _msg;
		private readonly IRefreshTokenService _refreshTokenService;

		public AuthService(
			RecruitDevContext context,
			IJwtService jwtService,
			ILogger<AuthService> logger,
			IConfiguration configuration,
			IMessageService messageService,
			IRefreshTokenService refreshTokenService)
		{
			_context = context;
			_jwtService = jwtService;
			_logger = logger;
			_configuration = configuration;
			_msg = messageService;
			_refreshTokenService = refreshTokenService;
		}

		public async Task<AuthResponseDto> Register(RegisterRequestDto request, string ipAddress)
		{
			try
			{
				// Kiểm tra email đã tồn tại
				var existing = await _context.Users
					.FirstOrDefaultAsync(x => x.Email == request.Email);

				if (existing != null)
					_msg.Throw(ErrorCode.EmailAlreadyExists, "EmailExists");

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

				if (request.Gender != null)
					user.Gender = request.Gender;
				if (request.PhoneNumber != null)
					user.PhoneNumber = request.PhoneNumber;
				if (request.DateOfBirth != null)
					user.DateOfBirth = request.DateOfBirth;

				// Tạo auth provider với password đã hash
				var provider = new AuthProvider
				{
					Id = Guid.NewGuid(),
					Provider = AuthProviderType.Email,
					ProviderUserId = request.Email,
					PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
					UserId = user.Id,
					User = user,
					CreatedAt = DateTime.UtcNow,
					ProviderEmail = request.Email
				};

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


				// Thêm vào database
				_context.Users.Add(user);
				_context.AuthProviders.Add(provider);
				_context.RefreshTokens.Add(tokenEntity);

				await _context.SaveChangesAsync();

				// Generate token
				var token = await _jwtService.GenerateToken(user);

				// Đọc expiry từ config
				var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 15);
				var expirySeconds = expiryMinutes * 60;

				// Log thành công
				_logger.LogInformation(_msg.Log("RegistrationSuccess"), request.Email);

				return new AuthResponseDto
				{
					AccessToken = token,
					UserId = user.Id.ToString(),
					Email = user.Email,
					FullName = user.FullName,
					ExpiresIn = expirySeconds
				};
			}
			catch (BusinessException)
			{
				throw; // Giữ nguyên cho controller xử lý
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, _msg.Log("RegistrationError"), request.Email);
				_msg.Throw(ErrorCode.InternalServerError, "RegistrationFailed", ex, ex.Message);
				return null; // Never reached
			}
		}

		public async Task<AuthResponseDto> Login(LoginRequestDto request, string ipAddress)
		{
			try
			{
				// Tìm auth provider theo email
				var provider = await _context.AuthProviders
					.Include(x => x.User)
					.FirstOrDefaultAsync(x => x.Provider == AuthProviderType.Email && x.ProviderUserId == request.Email);

				if (provider == null)
					_msg.Throw(ErrorCode.InvalidCredentials, "InvalidCredentials");

				// Kiểm tra password
				if (!BCrypt.Net.BCrypt.Verify(request.Password, provider.PasswordHash))
					_msg.Throw(ErrorCode.InvalidCredentials, "InvalidCredentials");

				var user = provider.User;

				// Kiểm tra trạng thái user
				if (user.Status != UserStatus.Active)
					_msg.Throw(ErrorCode.AccountLocked, "AccountLocked");


				provider.LastLoginAt = DateTime.UtcNow;
				provider.ProviderEmail = request.Email;
				user.LastLoginAt = DateTime.UtcNow;

				// Đọc expiry từ config
				var accessTokenExpiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
				var refreshTokenExpiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

				var refreshToken = _refreshTokenService.GenerateToken();

				// Revoke tất cả refresh tokens cũ của user
				await _context.RefreshTokens
				.Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
				.ExecuteUpdateAsync(setters => setters
					.SetProperty(rt => rt.IsRevoked, true)
					.SetProperty(rt => rt.RevokedAt, DateTime.UtcNow)
					.SetProperty(rt => rt.RevokedByIp, ipAddress)
					.SetProperty(rt => rt.ReplacedByToken, refreshToken)
				);

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

				_context.RefreshTokens.Add(tokenEntity);
				await _context.SaveChangesAsync();

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
			catch (BusinessException)
			{
				throw; // Giữ nguyên cho controller xử lý
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, _msg.Log("LoginError"), request.Email);
				_msg.Throw(ErrorCode.InternalServerError, "LoginFailed", ex, ex.Message);
				return null; // Never reached
			}
		}

		public async Task Logout(string refreshToken, string ipAddress)
		{
			try
			{
				// Tìm token trong database
				var token = await _context.RefreshTokens
					.Include(rt => rt.User)  // Load thêm thông tin user để log
					.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

				if (token != null && !token.IsRevoked)
				{
					// 1. Revoke token hiện tại
					token.IsRevoked = true;
					token.RevokedAt = DateTime.UtcNow;
					token.RevokedByIp = ipAddress;

					// 2. Cập nhật thời gian logout của user (optional)
					if (token.User != null)
					{
						token.User.LastLoginAt = null;
					}

					// 3. Lưu thay đổi
					await _context.SaveChangesAsync();

					// 4. Log hành động logout
					_logger.LogInformation($"User {token.User?.Email} logged out successfully from IP: {ipAddress} at {DateTime.UtcNow}");
				}
				else
				{
					// 5. Log warning nếu token không hợp lệ
					_logger.LogWarning($"Invalid logout attempt with token: {refreshToken} from IP: {ipAddress}");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error during logout for token: {refreshToken} from IP: {ipAddress}");
			}
		}

		public async Task<AuthResponseDto> RefreshToken(string refreshToken, string ipAddress)
		{
			try
			{
				// Tìm refresh token trong database
				var token = await _context.RefreshTokens
					.Include(rt => rt.User)
					.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

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

				// Tạo refresh token MỚI
				var newRefreshToken = _refreshTokenService.GenerateToken();
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

				// Revoke token CŨ (cái đang dùng)
				token.IsRevoked = true;
				token.RevokedAt = DateTime.UtcNow;
				token.RevokedByIp = ipAddress;
				token.ReplacedByToken = newRefreshToken;

				// Thêm token mới vào database
				_context.RefreshTokens.Add(newTokenEntity);
				await _context.SaveChangesAsync();

				// Generate access token MỚI
				var accessToken = await _jwtService.GenerateToken(user);
				var expirySeconds = accessTokenExpiryMinutes * 60;

				// Log thành công
				_logger.LogInformation($"Token refreshed for user: {user.Email} from IP: {ipAddress}");

				// Trả về AuthResponseDto chứa token mới
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
			catch (BusinessException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error refreshing token");
				_msg.Throw(ErrorCode.InternalServerError, "TokenRefreshFailed");
				return null;
			}
		}
	}
}