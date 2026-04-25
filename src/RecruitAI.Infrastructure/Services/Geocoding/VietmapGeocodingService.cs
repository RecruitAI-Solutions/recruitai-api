// RecruitAI.Infrastructure/Services/Geocoding/VietmapGeocodingService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using RecruitAI.Domain.Enums;
using RecruitAI.Infrastructure.Caching;
using RecruitAI.Shared.DTOs;
using RecruitAI.Shared.Interfaces;
using System.Text.Json;

namespace RecruitAI.Infrastructure.Services.Geocoding
{
	public class VietmapGeocodingService : IGeocodingService
	{
		private readonly HttpClient _autocompleteClient;
		private readonly HttpClient _placeClient;
		private readonly ILogger<VietmapGeocodingService> _logger;
		private readonly IRedisCacheService _cache;
		private readonly string _apiKey;
		private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
		private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;

		public VietmapGeocodingService(
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			ILogger<VietmapGeocodingService> logger,
			IRedisCacheService cache)
		{
			_autocompleteClient = httpClientFactory.CreateClient("VietmapAutocomplete");
			_placeClient = httpClientFactory.CreateClient("VietmapPlace");
			_apiKey = configuration["Vietmap:ApiKey"] ?? throw new InvalidOperationException("Vietmap API Key not configured");
			_logger = logger;
			_cache = cache;

			_retryPolicy = HttpPolicyExtensions
				.HandleTransientHttpError()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					onRetry: (outcome, timespan, retryCount, context) =>
					{
						_logger.LogWarning("Retry {RetryCount} after {Timespan}s due to {Error}",
							retryCount, timespan.TotalSeconds, outcome.Exception?.Message);
					});

			_circuitBreaker = HttpPolicyExtensions
				.HandleTransientHttpError()
				.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
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
				var cacheKey = $"geocoding:search:{text}:{limit}:{lat}:{lng}:{cityId}:{wardId}:{displayType}";
				var cached = await _cache.GetAsync<List<AddressDto>>(cacheKey, cancellationToken);
				if (cached != null)
				{
					_logger.LogInformation("Cache HIT for {CacheKey}", cacheKey);
					return cached;
				}

				var queryString = BuildQueryString(text, lat, lng, displayType);
				var url = $"?apikey={_apiKey}{queryString}";
				var response = await CallAutocompleteApiAsync(url, cancellationToken);

				if (string.IsNullOrEmpty(response))
					return new List<AddressDto>();

				var vietmapPlaces = JsonSerializer.Deserialize<List<VietmapPlace>>(response);
				if (vietmapPlaces == null)
					return new List<AddressDto>();

				var results = vietmapPlaces.Select(MapToAddressDto).ToList();

				if (limit.HasValue && results.Count > limit.Value)
					results = results.Take(limit.Value).ToList();

				if (!string.IsNullOrEmpty(cityId) || !string.IsNullOrEmpty(wardId))
					results = FilterResults(results, cityId, wardId);

				if (lat.HasValue && lng.HasValue && results.Any())
				{
					results = results.OrderBy(a => CalculateDistance(
						lat.Value, lng.Value, a.Location.Lat, a.Location.Lng)).ToList();
				}

				await _cache.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5), cancellationToken);
				return results;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error searching address: {Text}", text);
				return new List<AddressDto>();
			}
		}

		public async Task<AddressDto?> GetAddressDetailAsync(string refId, CancellationToken cancellationToken = default)
		{
			try
			{
				var cacheKey = $"geocoding:detail:{refId}";
				var cached = await _cache.GetAsync<AddressDto>(cacheKey, cancellationToken);
				if (cached != null) return cached;

				var url = $"/place/{refId}?apikey={_apiKey}";
				var response = await CallPlaceApiAsync(url, cancellationToken);

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

		public async Task<List<AddressDto>> ReverseGeocodingAsync(double lat, double lng, int? radius = 100, CancellationToken cancellationToken = default)
		{
			try
			{
				var cacheKey = $"geocoding:reverse:{lat}:{lng}:{radius}";
				var cached = await _cache.GetAsync<List<AddressDto>>(cacheKey, cancellationToken);
				if (cached != null) return cached;

				var url = $"/reverse?apikey={_apiKey}&lat={lat}&lng={lng}&radius={radius}";
				var response = await CallPlaceApiAsync(url, cancellationToken);

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

		public async Task<List<ProvinceDto>> GetProvincesAsync(string? searchText = null, CancellationToken cancellationToken = default)
		{
			var cacheKey = string.IsNullOrEmpty(searchText)
				? "geocoding:provinces:all"
				: $"geocoding:provinces:search:{searchText}";

			var cached = await _cache.GetAsync<List<ProvinceDto>>(cacheKey, cancellationToken);
			if (cached != null)
			{
				_logger.LogInformation("Cache HIT for provinces");
				return cached;
			}

			try
			{
				// Dùng Autocomplete API với layers=CITY để lấy danh sách tỉnh/thành
				var text = string.IsNullOrEmpty(searchText) ? "" : Uri.EscapeDataString(searchText);
				var url = $"?apikey={_apiKey}&text={text}&layers=CITY&display_type=5&limit=50";
				var response = await CallAutocompleteApiAsync(url, cancellationToken);

				if (string.IsNullOrEmpty(response))
					return new List<ProvinceDto>();

				var places = JsonSerializer.Deserialize<List<VietmapPlace>>(response);
				var result = places?.Select(p => new ProvinceDto
				{
					Id = p.Boundaries?.FirstOrDefault(b => b.Type == 0)?.Id.ToString() ?? p.RefId,
					Name = p.Name,
					Code = p.Boundaries?.FirstOrDefault(b => b.Type == 0)?.Id.ToString()
				}).DistinctBy(p => p.Id).ToList() ?? new();

				await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(6), cancellationToken);
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting provinces");
				return new List<ProvinceDto>();
			}
		}

		public async Task<List<DistrictDto>> GetDistrictsAsync(string provinceId, string? searchText = null, CancellationToken cancellationToken = default)
		{
			var cacheKey = $"geocoding:districts:{provinceId}:{searchText ?? "all"}";

			var cached = await _cache.GetAsync<List<DistrictDto>>(cacheKey, cancellationToken);
			if (cached != null)
			{
				_logger.LogInformation("Cache HIT for districts of province {ProvinceId}", provinceId);
				return cached;
			}

			try
			{
				// Dùng Autocomplete API với layers=DIST và cityId để lấy quận/huyện theo tỉnh
				var text = string.IsNullOrEmpty(searchText) ? "" : Uri.EscapeDataString(searchText);
				var url = $"?apikey={_apiKey}&text={text}&layers=DIST&cityId={provinceId}&display_type=5&limit=50";
				var response = await CallAutocompleteApiAsync(url, cancellationToken);

				if (string.IsNullOrEmpty(response))
					return new List<DistrictDto>();

				var places = JsonSerializer.Deserialize<List<VietmapPlace>>(response);
				var result = places?.Select(p => new DistrictDto
				{
					Id = p.Boundaries?.FirstOrDefault(b => b.Type == 1)?.Id.ToString() ?? p.RefId,
					Name = p.Name,
					ProvinceId = provinceId,
					Code = p.Boundaries?.FirstOrDefault(b => b.Type == 1)?.Id.ToString()
				}).DistinctBy(d => d.Id).ToList() ?? new();

				await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(6), cancellationToken);
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting districts for province {ProvinceId}", provinceId);
				return new List<DistrictDto>();
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

			query += displayType.HasValue
				? $"&display_type={(int)displayType.Value}"
				: "&display_type=5";

			return query;
		}

		private async Task<string?> CallAutocompleteApiAsync(string url, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogDebug("Calling Vietmap Autocomplete API: {Url}", url);
				var response = await _retryPolicy.ExecuteAsync(async () =>
					await _circuitBreaker.ExecuteAsync(async () =>
						await _autocompleteClient.GetAsync(url, cancellationToken)
					)
				);

				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					_logger.LogError("Vietmap Autocomplete API returned {StatusCode}. Error: {Error}",
						response.StatusCode, errorContent);
					return null;
				}

				return await response.Content.ReadAsStringAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error calling Vietmap Autocomplete API: {Url}", url);
				return null;
			}
		}

		private async Task<string?> CallPlaceApiAsync(string url, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogDebug("Calling Vietmap Place API: {Url}", url);
				var response = await _retryPolicy.ExecuteAsync(async () =>
					await _circuitBreaker.ExecuteAsync(async () =>
						await _placeClient.GetAsync(url, cancellationToken)
					)
				);

				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					_logger.LogError("Vietmap Place API returned {StatusCode}. Error: {Error}",
						response.StatusCode, errorContent);
					return null;
				}

				return await response.Content.ReadAsStringAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error calling Vietmap Place API: {Url}", url);
				return null;
			}
		}

		private AddressDto MapToAddressDto(VietmapPlace place) => new()
		{
			RefId = place.RefId,
			FullAddress = place.Address,
			Display = place.Display,
			Location = new LocationDto { Lat = 0, Lng = 0 },
			Boundaries = place.Boundaries?.Select(b => new BoundaryDto
			{
				Type = (BoundaryType)b.Type,
				Name = b.Name,
				Prefix = b.Prefix ?? string.Empty,
				Code = b.Id.ToString()
			}).ToList() ?? new(),
			Formats = new AddressFormatDto
			{
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

		private List<AddressDto> FilterResults(List<AddressDto> results, string? cityId, string? wardId) =>
			results.Where(a =>
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

		private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
		{
			var R = 6371e3;
			var φ1 = lat1 * Math.PI / 180;
			var φ2 = lat2 * Math.PI / 180;
			var Δφ = (lat2 - lat1) * Math.PI / 180;
			var Δλ = (lon2 - lon1) * Math.PI / 180;

			var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
					Math.Cos(φ1) * Math.Cos(φ2) *
					Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return R * c;
		}
	}
}