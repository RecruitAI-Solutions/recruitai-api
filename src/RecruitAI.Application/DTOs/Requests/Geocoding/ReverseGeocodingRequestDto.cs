// RecruitAI.Application/DTOs/Requests/Geocoding/ReverseGeocodingRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace RecruitAI.Application.DTOs.Requests.Geocoding
{
	public class ReverseGeocodingRequestDto
	{
		[Required]
		[Range(-90, 90)]
		public double Lat { get; set; }

		[Required]
		[Range(-180, 180)]
		public double Lng { get; set; }

		[Range(1, 1000)]
		public int? Radius { get; set; } = 100;
	}
}