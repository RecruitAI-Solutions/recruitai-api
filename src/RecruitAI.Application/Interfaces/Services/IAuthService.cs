using RecruitAI.Application.DTOs.Auths;
using RecruitAI.Application.DTOs.Requests.Auths;
using RecruitAI.Application.DTOs.Responses.Auths;

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
		Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
		Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
		Task<SendVerificationEmailResponseDto> SendVerificationEmailAsync(SendVerificationEmailRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
		Task<VerifyEmailResponseDto> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken = default);
		Task<AuthResponseDto> ExternalLoginAsync(string provider, string providerUserId, string email, string name, string ipAddress, CancellationToken cancellationToken = default);
	}
}
