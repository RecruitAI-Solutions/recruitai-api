using Microsoft.Extensions.DependencyInjection;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Services;
using RecruitAI.Domain.Interfaces.Services;
using RecruitAI.Domain.Services;

namespace RecruitAI.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register application services here
            // e.g., services.AddScoped<ITestService, TestService>();
            services.AddScoped<ITestService, TestService>();
            services.AddScoped<ITestDomainService, TestDomainService>();

            return services;
        }
    }
}