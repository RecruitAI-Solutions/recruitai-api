using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Infrastructure.BackgroundServices
{
	public class AvatarCleanupBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<AvatarCleanupBackgroundService> _logger;
		private readonly IStorageSettings _storageSettings;

		public AvatarCleanupBackgroundService(
			IServiceScopeFactory scopeFactory,
			ILogger<AvatarCleanupBackgroundService> logger,
			IStorageSettings storageSettings)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
			_storageSettings = storageSettings;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Avatar Cleanup Background Service started");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

					_logger.LogInformation("Running avatar cleanup job...");

					using (var scope = _scopeFactory.CreateScope())
					{
						var cleanupService = scope.ServiceProvider.GetRequiredService<IAvatarCleanupService>();
						await cleanupService.CleanupOrphanedAvatarsAsync(force: false, stoppingToken);
					}

					_logger.LogInformation("Avatar cleanup job completed");
				}
				catch (TaskCanceledException)
				{
					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error running avatar cleanup job");
				}
			}

			_logger.LogInformation("Avatar Cleanup Background Service stopped");
		}
	}
}