using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecruitAI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace RecruitAI.Infrastructure.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddInfrastructure(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			// Register DbContext
			services.AddDbContext<RecruitDevContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

			// Register Repositories
			//services.AddScoped<IUserRepository, UserRepository>();

			return services;
		}
	}

}
