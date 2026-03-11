using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Interfaces.Services
{
	public interface IJwtService
	{
		/// <summary>
		/// Tạo JWT token cho user
		/// </summary>
		Task<string> GenerateToken(User user, CancellationToken cancellationToken = default);

		/// <summary>
		/// Xác thực JWT token
		/// </summary>
		Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
	}
}