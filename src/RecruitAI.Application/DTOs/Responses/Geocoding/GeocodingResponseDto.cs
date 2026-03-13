// RecruitAI.Application/DTOs/Responses/Geocoding/GeocodingResponseDto.cs
namespace RecruitAI.Application.DTOs.Responses.Geocoding
{
	public class GeocodingResponseDto
	{
		public bool Success { get; set; }
		public string? Message { get; set; }
		public List<AddressDto> Data { get; set; } = new();
		public string? TraceId { get; set; }
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
	}
}