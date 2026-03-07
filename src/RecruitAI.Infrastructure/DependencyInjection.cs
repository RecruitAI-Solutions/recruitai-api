using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;
using RecruitAI.Infrastructure.Repositories;

namespace RecruitAI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<RecruitDevContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register Repositories
            services.AddScoped<ITestRepository, TestRepository>();

            return services;
        }
    }

}
