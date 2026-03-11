using RecruitAI.Application.DTOs;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;

namespace RecruitAI.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> Login(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
        Task Logout(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> RefreshToken(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
        Task<UserProfileDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ChangePasswordResponseDto> ChangePasswordAsync(ChangePasswordRequestDto request, Guid userId, CancellationToken cancellationToken = default);
    }
}
