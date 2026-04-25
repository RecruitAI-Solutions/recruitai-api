namespace RecruitAI.Shared.DTOs
{
	public class VietmapDistrict
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public int ProvinceId { get; set; }
		public string? Code { get; set; }
	}

}
