using RecruitAI.Domain.Enums;

namespace RecruitAI.Shared.DTOs
{
	public class BoundaryDto
	{
		public BoundaryType Type { get; set; } // 0: tỉnh, 1: quận/huyện, 2: phường/xã
		public string Name { get; set; } = string.Empty;
		public string? Prefix { get; set; }
		public string Code { get; set; } = string.Empty;
	}
}