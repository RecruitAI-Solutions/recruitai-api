using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Domain.Interfaces.Services;
using RecruitAI.Infrastructure.Caching;
using RecruitAI.Infrastructure.Data;
using RecruitAI.Infrastructure.Repositories;
using RecruitAI.Infrastructure.Services;
using RecruitAI.Infrastructure.Services.Geocoding;
using RecruitAI.Infrastructure.Settings;
using RecruitAI.Shared.Interfaces;

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
			services.AddScoped<ICVRepository, CVRepository>();
			services.AddScoped<IJobRepository, JobRepository>();
			services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
			services.AddScoped<IJobApplicationMatchRepository, JobApplicationMatchRepository>();
			services.AddScoped<IAuditLogRepository, AuditLogRepository>();
			services.AddScoped<ICompanyRepository, CompanyRepository>();
			services.AddScoped<INotificationRepository, NotificationRepository>();

			services.AddScoped<IWorkContext, WorkContext>();
			services.AddScoped<IEmailTemplateService, EmailTemplateService>();
			services.AddSingleton<FileSystemWatcher>();

			// Redis Cache
			services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = configuration.GetConnectionString("Redis");
				options.InstanceName = configuration["Redis:InstanceName"];
			});
			services.AddScoped<IRedisCacheService, RedisCacheService>();

			// Geocoding Service
			services.AddHttpClient("Vietmap", client =>
			{
				client.BaseAddress = new Uri(configuration["Vietmap:BaseUrl"] ?? "https://maps.vietmap.vn/api/v4");
				client.DefaultRequestHeaders.Add("Accept", "application/json");
				client.Timeout = TimeSpan.FromSeconds(10);
			});
			services.AddScoped<IGeocodingService, VietmapGeocodingService>();


			//Register other infrastructure services
			services.AddScoped<IJwtService, JwtService>();
			services.AddScoped<IPdfService, PdfService>();
			services.AddScoped<IAppUrlService, AppUrlService>();
			services.AddHostedService<FeaturedJobBackgroundService>();
			services.AddScoped<IExportService, ExportService>();
			//Settings
			services.AddSingleton<IStorageSettings, StorageSettings>();

			return services;
		}
	}

}
