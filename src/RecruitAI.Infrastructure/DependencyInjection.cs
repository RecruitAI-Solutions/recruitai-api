using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecruitAI.Infrastructure.Data;
using RecruitAI.Infrastructure.Repositories;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Repositories;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Infrastructure.Services;

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
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<ITestRepository, TestRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IAuthProviderRepository, AuthProviderRepository>();
			services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
			services.AddScoped<IWorkContext, WorkContext>();
			services.AddScoped<IEmailTemplateService, EmailTemplateService>();

			//Register other infrastructure services
			services.AddScoped<IJwtService, JwtService>();

			return services;
		}
	}

}
