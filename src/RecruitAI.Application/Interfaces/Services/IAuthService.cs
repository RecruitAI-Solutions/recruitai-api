using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;

namespace RecruitAI.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterRequestDto request);
        Task<AuthResponseDto> Login(LoginRequestDto request);
    }
}
