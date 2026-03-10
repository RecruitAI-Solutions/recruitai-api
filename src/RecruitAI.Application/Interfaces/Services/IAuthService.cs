using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;

namespace RecruitAI.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterRequestDto request, string ipAddress);
        Task<AuthResponseDto> Login(LoginRequestDto request, string ipAddress);
        Task Logout(string refreshToken, string ipAddress);
        Task<AuthResponseDto> RefreshToken(string refreshToken, string ipAddress);

    }
}
