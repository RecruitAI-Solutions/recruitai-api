// RecruitAI.Infrastructure/Services/Geocoding/VietmapModels.cs
using System.Text.Json.Serialization;

namespace RecruitAI.Infrastructure.Services.Geocoding
{
	// API trả về mảng trực tiếp
	public class VietmapPlace
	{
		[JsonPropertyName("ref_id")]
		public string RefId { get; set; } = string.Empty;

		[JsonPropertyName("distance")]
		public double Distance { get; set; }

		[JsonPropertyName("address")]
		public string Address { get; set; } = string.Empty;

		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("display")]
		public string Display { get; set; } = string.Empty;

		[JsonPropertyName("boundaries")]
		public List<VietmapBoundary>? Boundaries { get; set; }

		[JsonPropertyName("categories")]
		public List<object>? Categories { get; set; }

		[JsonPropertyName("entry_points")]
		public List<object>? EntryPoints { get; set; }

		[JsonPropertyName("data_new")]
		public VietmapPlaceNew? DataNew { get; set; }
	}

	public class VietmapPlaceNew
	{
		[JsonPropertyName("ref_id")]
		public string RefId { get; set; } = string.Empty;

		[JsonPropertyName("distance")]
		public double Distance { get; set; }

		[JsonPropertyName("address")]
		public string Address { get; set; } = string.Empty;

		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("display")]
		public string Display { get; set; } = string.Empty;

		[JsonPropertyName("boundaries")]
		public List<VietmapBoundary>? Boundaries { get; set; }

		[JsonPropertyName("categories")]
		public List<object>? Categories { get; set; }

		[JsonPropertyName("entry_points")]
		public List<object>? EntryPoints { get; set; }
	}

	public class VietmapBoundary
	{
		[JsonPropertyName("type")]
		public int Type { get; set; }

		[JsonPropertyName("id")]
		public long Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("prefix")]
		public string? Prefix { get; set; }

		[JsonPropertyName("full_name")]
		public string FullName { get; set; } = string.Empty;
	}
}