namespace RecruitAI.Application.DTOs.AI
{
	public class AIMatchRequestDto
	{
		public string CvText { get; set; } = string.Empty;
		public string JobTitle { get; set; } = string.Empty;
		public string JobDescription { get; set; } = string.Empty;
		public string JobRequirements { get; set; } = string.Empty;
		public decimal? SalaryMin { get; set; }
		public decimal? SalaryMax { get; set; }
		public string Location { get; set; } = string.Empty;
	}
}