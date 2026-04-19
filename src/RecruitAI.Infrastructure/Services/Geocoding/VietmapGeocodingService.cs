// RecruitAI.Infrastructure/Services/Geocoding/VietmapGeocodingService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using RecruitAI.Application.DTOs.Responses.Geocoding;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Infrastructure.Caching;
using RecruitAI.Infrastructure.Services.Geocoding;
using System.Text.Json;

namespace RecruitAI.Infrastructure.Services.Geocoding
{
	public class VietmapGeocodingService : IGeocodingService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<VietmapGeocodingService> _logger;
		private readonly IRedisCacheService _cache;
		private readonly string _apiKey;
		private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
		private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;
		private readonly string _baseUrl;

		public VietmapGeocodingService(
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			ILogger<VietmapGeocodingService> logger,
			IRedisCacheService cache)
		{
			_httpClient = httpClientFactory.CreateClient("Vietmap");
			_apiKey = configuration["Vietmap:ApiKey"] ?? throw new InvalidOperationException("Vietmap API Key not configured");
			_logger = logger;
			_cache = cache;
			//_baseUrl = configuration["Vietmap:BaseUrl"] ?? "https://maps.vietmap.vn/api/autocomplete/v4";

			// Retry policy (3 lần, exponential backoff)
			_retryPolicy = HttpPolicyExtensions
				.HandleTransientHttpError()
				.WaitAndRetryAsync(
					3,
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					onRetry: (outcome, timespan, retryCount, context) =>
					{
						_logger.LogWarning("Retry {RetryCount} after {Timespan}s due to {Error}",
							retryCount, timespan.TotalSeconds, outcome.Exception?.Message);
					});

			// Circuit breaker (ngắt sau 5 lỗi, nghỉ 30 giây)
			_circuitBreaker = HttpPolicyExtensions
				.HandleTransientHttpError()
				.CircuitBreakerAsync(
					5,
					TimeSpan.FromSeconds(30),
					onBreak: (ex, time) => _logger.LogError("Circuit broken for {BreakTime}s", time.TotalSeconds),
					onReset: () => _logger.LogInformation("Circuit reset"),
					onHalfOpen: () => _logger.LogWarning("Circuit half-open"));
		}

		public async Task<List<AddressDto>> SearchAddressAsync(
			string text,
			int? limit = 5,
			double? lat = null,
			double? lng = null,
			string? cityId = null,
			string? wardId = null,
			DisplayType? displayType = DisplayType.BothNewWithOld,
			CancellationToken cancellationToken = default)
		{
			try
			{
				// 1. Kiểm tra cache
				var cacheKey = $"geocoding:search:{text}:{limit}:{lat}:{lng}:{cityId}:{wardId}:{displayType}";
				var cached = await _cache.GetAsync<List<AddressDto>>(cacheKey, cancellationToken);
				if (cached != null)
				{
					_logger.LogInformation("Cache HIT for {CacheKey}", cacheKey);
					return cached;
				}

				// 2. Gọi Vietmap API (không truyền limit vì API không hỗ trợ)
				var queryString = BuildQueryString(text, lat, lng, displayType);
				var relativeUrl = $"?apikey={_apiKey}{queryString}";
				var response = await CallVietmapApiAsync(relativeUrl, cancellationToken);

				if (string.IsNullOrEmpty(response))
					return new List<AddressDto>();

				var vietmapPlaces = JsonSerializer.Deserialize<List<VietmapPlace>>(response);
				if (vietmapPlaces == null)
					return new List<AddressDto>();

				// 3. Transform dữ liệu
				var results = vietmapPlaces.Select(MapToAddressDto).ToList();

				// 4. GIỚI HẠN SỐ LƯỢNG KẾT QUẢ THEO LIMIT
				if (limit.HasValue && results.Count > limit.Value)
				{
					_logger.LogInformation("Limiting results from {OriginalCount} to {Limit}",
						results.Count, limit.Value);
					results = results.Take(limit.Value).ToList();
				}

				// 5. Lọc kết quả (nếu có)
				if (!string.IsNullOrEmpty(cityId) || !string.IsNullOrEmpty(wardId))
				{
					results = FilterResults(results, cityId, wardId);
				}

				// 6. Sắp xếp theo khoảng cách nếu có tọa độ
				if (lat.HasValue && lng.HasValue && results.Any())
				{
					results = results.OrderBy(a => CalculateDistance(
						lat.Value, lng.Value, a.Location.Lat, a.Location.Lng)).ToList();
				}

				// 7. Lưu cache (5 phút)
				await _cache.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5), cancellationToken);

				return results;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error searching address: {Text}", text);
				return new List<AddressDto>();
			}
		}

		public async Task<AddressDto?> GetAddressDetailAsync(
			string refId,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var cacheKey = $"geocoding:detail:{refId}";
				var cached = await _cache.GetAsync<AddressDto>(cacheKey, cancellationToken);
				if (cached != null) return cached;

				var relativeUrl = $"/place/{refId}?apikey={_apiKey}";
				var response = await CallVietmapApiAsync(relativeUrl, cancellationToken);

				if (string.IsNullOrEmpty(response)) return null;

				var place = JsonSerializer.Deserialize<VietmapPlace>(response);
				if (place == null) return null;

				var result = MapToAddressDto(place);
				await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting address detail: {RefId}", refId);
				return null;
			}
		}

		public async Task<List<AddressDto>> ReverseGeocodingAsync(
			double lat,
			double lng,
			int? radius = 100,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var cacheKey = $"geocoding:reverse:{lat}:{lng}:{radius}";
				var cached = await _cache.GetAsync<List<AddressDto>>(cacheKey, cancellationToken);
				if (cached != null) return cached;

				var relativeUrl = $"/reverse?apikey={_apiKey}&lat={lat}&lng={lng}&radius={radius}";
				var response = await CallVietmapApiAsync(relativeUrl, cancellationToken);

				if (string.IsNullOrEmpty(response))
					return new List<AddressDto>();

				var vietmapPlaces = JsonSerializer.Deserialize<List<VietmapPlace>>(response);
				var results = vietmapPlaces?.Select(MapToAddressDto).ToList() ?? new();

				await _cache.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5), cancellationToken);

				return results;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error reverse geocoding: {Lat},{Lng}", lat, lng);
				return new List<AddressDto>();
			}
		}

		private string BuildQueryString(string text, double? lat, double? lng, DisplayType? displayType)
		{
			var query = $"&text={Uri.EscapeDataString(text)}";

			if (lat.HasValue && lng.HasValue)
			{
				query += $"&focus={lat.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
						$"{lng.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
			}

			// Sử dụng display_type theo enum
			if (displayType.HasValue)
			{
				query += $"&display_type={(int)displayType.Value}";
			}
			else
			{
				// Mặc định dùng 5 (both new with old)
				query += "&display_type=5";
			}

			return query;
		}

		private async Task<string?> CallVietmapApiAsync(string relativeUrl, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogDebug("Calling Vietmap API endpoint: {Endpoint}", relativeUrl);
				var response = await _retryPolicy.ExecuteAsync(async () =>
					await _circuitBreaker.ExecuteAsync(async () =>
						await _httpClient.GetAsync(relativeUrl, cancellationToken)
					)
				);

				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					_logger.LogError("Vietmap API returned {StatusCode} for {Endpoint}. Error: {Error}",
						response.StatusCode, relativeUrl, errorContent);
					return null;
				}

				return await response.Content.ReadAsStringAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error calling Vietmap API: {Endpoint}", relativeUrl);
				return null;
			}
		}

		private AddressDto MapToAddressDto(VietmapPlace place)
		{
			return new AddressDto
			{
				RefId = place.RefId,
				FullAddress = place.Address,
				Display = place.Display,
				Location = new LocationDto
				{
					Lat = 0, // API autocomplete không trả về location
					Lng = 0
				},
				Boundaries = place.Boundaries?.Select(b => new BoundaryDto
				{
					Type = (BoundaryType)b.Type,
					Name = b.Name,
					Prefix = b.Prefix ?? string.Empty,
					Code = b.Id.ToString()
				}).ToList() ?? new(),
				Formats = new AddressFormatDto
				{
					// Format mới từ data_new nếu có
					New = place.DataNew != null ? new FormatDetailDto
					{
						Address = place.DataNew.Address,
						Boundaries = place.DataNew.Boundaries?.Select(b => new BoundaryInfoDto
						{
							Type = b.Type,
							Name = b.Name,
							Code = b.Id.ToString()
						}).ToList() ?? new()
					} : null,

					// Format cũ từ place chính hoặc data_old
					Old = place.DataOld != null ? new FormatDetailDto
					{
						Address = place.DataOld.Address,
						Boundaries = place.DataOld.Boundaries?.Select(b => new BoundaryInfoDto
						{
							Type = b.Type,
							Name = b.Name,
							Code = b.Id.ToString()
						}).ToList() ?? new()
					} : new FormatDetailDto
					{
						Address = place.Address,
						Boundaries = place.Boundaries?.Select(b => new BoundaryInfoDto
						{
							Type = b.Type,
							Name = b.Name,
							Code = b.Id.ToString()
						}).ToList() ?? new()
					}
				}
			};
		}

		private List<AddressDto> FilterResults(List<AddressDto> results, string? cityId, string? wardId)
		{
			return results.Where(a =>
			{
				if (!string.IsNullOrEmpty(cityId))
				{
					var hasCity = a.Boundaries.Any(b => b.Type == BoundaryType.Province && b.Code == cityId);
					if (!hasCity) return false;
				}

				if (!string.IsNullOrEmpty(wardId))
				{
					var hasWard = a.Boundaries.Any(b => b.Type == BoundaryType.Ward && b.Code == wardId);
					if (!hasWard) return false;
				}

				return true;
			}).ToList();
		}

		private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
		{
			// Công thức Haversine tính khoảng cách giữa 2 tọa độ
			var R = 6371e3; // Bán kính trái đất (mét)
			var φ1 = lat1 * Math.PI / 180;
			var φ2 = lat2 * Math.PI / 180;
			var Δφ = (lat2 - lat1) * Math.PI / 180;
			var Δλ = (lon2 - lon1) * Math.PI / 180;

			var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
					Math.Cos(φ1) * Math.Cos(φ2) *
					Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return R * c; // Khoảng cách (mét)
		}
	}
}