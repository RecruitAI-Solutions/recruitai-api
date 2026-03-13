// RecruitAI.Application/Interfaces/Services/IGeocodingService.cs
using RecruitAI.Application.DTOs.Responses.Geocoding;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Interfaces.Services
{
	public interface IGeocodingService
	{
		/// <summary>
		/// Tìm kiếm địa chỉ theo text
		/// </summary>
		/// <param name="text">Địa chỉ cần tìm (VD: "15 đường số 3")</param>
		/// <param name="limit">Số lượng kết quả tối đa</param>
		/// <param name="lat">Vĩ độ để ưu tiên kết quả gần</param>
		/// <param name="lng">Kinh độ để ưu tiên kết quả gần</param>
		/// <param name="cityId">Mã tỉnh/thành để lọc</param>
		/// <param name="wardId">Mã phường/xã để lọc</param>
		/// <param name="displayType">Kiểu hiển thị (1: mới, 2: cũ, 6: cả hai)</param>
		/// <param name="cancellationToken">Cancellation token</param>
		Task<List<AddressDto>> SearchAddressAsync(
			string text,
			int? limit = 10,
			double? lat = null,
			double? lng = null,
			string? cityId = null,
			string? wardId = null,
			DisplayType? displayType = DisplayType.BothNewWithOld,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Lấy chi tiết địa chỉ theo ID
		/// </summary>
		/// <param name="refId">ID tham chiếu của địa chỉ</param>
		/// <param name="cancellationToken">Cancellation token</param>
		Task<AddressDto?> GetAddressDetailAsync(
			string refId,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Reverse geocoding - từ tọa độ ra địa chỉ
		/// </summary>
		/// <param name="lat">Vĩ độ</param>
		/// <param name="lng">Kinh độ</param>
		/// <param name="radius">Bán kính tìm kiếm (mét)</param>
		/// <param name="cancellationToken">Cancellation token</param>
		Task<List<AddressDto>> ReverseGeocodingAsync(
			double lat,
			double lng,
			int? radius = 100,
			CancellationToken cancellationToken = default);
	}
}