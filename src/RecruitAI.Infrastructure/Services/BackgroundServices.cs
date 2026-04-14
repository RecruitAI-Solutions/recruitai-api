using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;

namespace RecruitAI.Infrastructure.Services;

public class FeaturedJobBackgroundService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<FeaturedJobBackgroundService> _logger;
	private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Chạy mỗi 1 giờ

	public FeaturedJobBackgroundService(
		IServiceScopeFactory scopeFactory,
		ILogger<FeaturedJobBackgroundService> logger)
	{
		_scopeFactory = scopeFactory;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("FeaturedJobBackgroundService is starting");

		// Chạy ngay khi khởi động
		await UpdateFeaturedJobsAsync(stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(_interval, stoppingToken);
				await UpdateFeaturedJobsAsync(stoppingToken);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating featured jobs");
			}
		}

		_logger.LogInformation("FeaturedJobBackgroundService is stopping");
	}

	private async Task UpdateFeaturedJobsAsync(CancellationToken cancellationToken)
	{
		using var scope = _scopeFactory.CreateScope();
		var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

		_logger.LogInformation("Calculating featured jobs...");

		// Lấy top 10 job có điểm cao nhất
		var featuredJobs = await uow.Jobs.GetTopJobsByScoreAsync(10, cancellationToken);

		// Reset tất cả IsFeatured = false
		await uow.Jobs.ResetAllFeaturedAsync(cancellationToken);

		// Đánh dấu top jobs
		for (int i = 0; i < featuredJobs.Count; i++)
		{
			featuredJobs[i].IsFeatured = true;
			featuredJobs[i].FeaturedOrder = i + 1;
		}

		if (featuredJobs.Any())
		{
			await uow.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Updated {Count} featured jobs", featuredJobs.Count);
		}
		else
		{
			_logger.LogWarning("No jobs found to mark as featured");
		}
	}
}