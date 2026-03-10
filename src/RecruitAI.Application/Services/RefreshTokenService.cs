using RecruitAI.Application.Interfaces.Services;
using System.Security.Cryptography;

namespace RecruitAI.Application.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        public string GenerateToken()
        {
            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
