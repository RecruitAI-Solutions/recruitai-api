using RecruitAI.Shared.DTOs;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Shared.Interfaces
{
	// RecruitAI.Shared/Interfaces/IGeocodingService.cs
	public interface IGeocodingService
	{
		Task<List<AddressDto>> SearchAddressAsync(string text, int? limit = 5, double? lat = null, double? lng = null, string? cityId = null, string? wardId = null, DisplayType? displayType = DisplayType.BothNewWithOld, CancellationToken cancellationToken = default);
		Task<AddressDto?> GetAddressDetailAsync(string refId, CancellationToken cancellationToken = default);
		Task<List<AddressDto>> ReverseGeocodingAsync(double lat, double lng, int? radius = 100, CancellationToken cancellationToken = default);
		Task<List<ProvinceDto>> GetProvincesAsync(string? searchText = null, CancellationToken cancellationToken = default);
		Task<List<DistrictDto>> GetDistrictsAsync(string provinceId, string? searchText = null, CancellationToken cancellationToken = default);
	}
}