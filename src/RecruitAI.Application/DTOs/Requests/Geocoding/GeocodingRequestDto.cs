// RecruitAI.Application/DTOs/Requests/Geocoding/GeocodingRequestDto.cs
using RecruitAI.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RecruitAI.Application.DTOs.Requests.Geocoding
{
	public class GeocodingRequestDto
	{
		[Required(ErrorMessage = "Vui lòng nhập địa chỉ cần tìm")]
		[MinLength(3, ErrorMessage = "Địa chỉ phải có ít nhất 3 ký tự")]
		public string Text { get; set; } = string.Empty;

		[Range(1, 50)]
		public int? Limit { get; set; } = 10;

		public double? Lat { get; set; }
		public double? Lng { get; set; }

		[RegularExpression(@"^\d+$", ErrorMessage = "Mã tỉnh không hợp lệ")]
		public string? CityId { get; set; }

		[RegularExpression(@"^\d+$", ErrorMessage = "Mã phường/xã không hợp lệ")]
		public string? WardId { get; set; }

		[Range(1, 6)]
		public DisplayType DisplayType { get; set; } = DisplayType.Both;
	}
}