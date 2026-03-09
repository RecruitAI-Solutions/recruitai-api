using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Interfaces.Services
{
    public interface IJwtService
    {
        Task<string> GenerateToken(User user);

    }
}
