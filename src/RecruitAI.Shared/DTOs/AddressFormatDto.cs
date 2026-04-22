namespace RecruitAI.Shared.DTOs
{
	public class AddressFormatDto
	{
		public FormatDetailDto? New { get; set; }
		public FormatDetailDto Old { get; set; } = new();
	}

	public class FormatDetailDto
	{
		public string Address { get; set; } = string.Empty;
		public List<BoundaryInfoDto> Boundaries { get; set; } = new();
	}

	public class BoundaryInfoDto
	{
		public int Type { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Code { get; set; } = string.Empty;
	}
}