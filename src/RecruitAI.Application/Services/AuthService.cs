using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
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

        public AuthService(
            RecruitDevContext context,
            IJwtService jwtService,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IMessageService messageService)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _configuration = configuration;
            _msg = messageService;
        }

        public async Task<AuthResponseDto> Register(RegisterRequestDto request)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                var existing = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email == request.Email);

                if (existing != null)
                    throw new Exception(_msg.Business("EmailExists"));

                // Tạo user mới
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FullName,
                    CreatedAt = DateTime.UtcNow,
                    Status = UserStatus.Active
                };

                // Tạo auth provider với password đã hash
                var provider = new AuthProvider
                {
                    Id = Guid.NewGuid(),
                    Provider = AuthProviderType.Email,
                    ProviderUserId = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                };

                // Thêm vào database
                _context.Users.Add(user);
                _context.AuthProviders.Add(provider);

                await _context.SaveChangesAsync();

                // Kiểm tra user trước khi tạo token
                if (user == null || user.Id == Guid.Empty)
                    throw new Exception(_msg.Business("UserCreationFailed"));

                // Generate token
                var token = await _jwtService.GenerateToken(user);

                // Đọc expiry từ config
                var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 15);

                // Tính số giây còn lại
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
            catch (Exception ex)
            {
                _logger.LogError(ex, _msg.Log("RegistrationError"), request.Email);
                throw new Exception(_msg.Business("RegistrationFailed", ex.Message));
            }
        }

        public async Task<AuthResponseDto> Login(LoginRequestDto request)
        {
            try
            {
                // Tìm auth provider theo email
                var provider = await _context.AuthProviders
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Provider == AuthProviderType.Email && x.ProviderUserId == request.Email);

                if (provider == null)
                    throw new Exception(_msg.Business("InvalidCredentials"));

                // Kiểm tra password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, provider.PasswordHash))
                    throw new Exception(_msg.Business("InvalidCredentials"));

                var user = provider.User;

                // Kiểm tra user trước khi tạo token
                if (user == null || user.Id == Guid.Empty)
                    throw new Exception(_msg.Business("UserNotFound"));

                // Generate token
                var token = await _jwtService.GenerateToken(user);

                // Đọc expiry từ config
                var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 15);

                // Tính số giây còn lại
                var expirySeconds = expiryMinutes * 60;

                // Log thành công
                _logger.LogInformation(_msg.Log("LoginSuccess"), request.Email);

                return new AuthResponseDto
                {
                    AccessToken = token,
                    UserId = user.Id.ToString(),
                    Email = user.Email,
                    FullName = user.FullName,
                    ExpiresIn = expirySeconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _msg.Log("LoginError"), request.Email);
                throw new Exception(_msg.Business("LoginFailed", ex.Message));
            }
        }
    }
}