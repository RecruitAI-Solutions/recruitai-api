// RecruitAI.Infrastructure/Caching/RedisCacheService.cs
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace RecruitAI.Infrastructure.Caching
{
	public class RedisCacheService : IRedisCacheService
	{
		private readonly IDistributedCache _cache;
		private readonly ILogger<RedisCacheService> _logger;

		public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
		{
			try
			{
				var data = await _cache.GetStringAsync(key, cancellationToken);
				if (data == null)
				{
					_logger.LogDebug("Cache MISS for {Key}", key);
					return default;
				}

				_logger.LogDebug("Cache HIT for {Key}", key);
				return JsonSerializer.Deserialize<T>(data);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Redis get failed for {Key}", key);
				return default;
			}
		}

		public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
		{
			try
			{
				var options = new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = expiration
				};
				var data = JsonSerializer.Serialize(value);
				await _cache.SetStringAsync(key, data, options, cancellationToken);
				_logger.LogDebug("Cached {Key} for {Expiration}", key, expiration);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Redis set failed for {Key}", key);
			}
		}

		public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
		{
			try
			{
				await _cache.RemoveAsync(key, cancellationToken);
				_logger.LogDebug("Removed cache for {Key}", key);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Redis remove failed for {Key}", key);
			}
		}
	}
}