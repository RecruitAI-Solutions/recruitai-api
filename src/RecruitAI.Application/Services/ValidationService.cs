using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RecruitAI.Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly RecruitDevContext _context;

        public ValidationService(RecruitDevContext context)
        {
            _context = context;
        }

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellation = default)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email, cancellation);
        }
    }

}
