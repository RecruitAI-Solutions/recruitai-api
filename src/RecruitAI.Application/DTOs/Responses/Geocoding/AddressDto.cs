// RecruitAI.Application/DTOs/Responses/Geocoding/AddressDto.cs
namespace RecruitAI.Application.DTOs.Responses.Geocoding
{
	public class AddressDto
	{
		public string RefId { get; set; } = string.Empty;
		public string FullAddress { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
		public LocationDto Location { get; set; } = new();
		public double? Distance { get; set; }
		public List<BoundaryDto> Boundaries { get; set; } = new();
		public AddressFormatDto Formats { get; set; } = new();
	}
}