namespace RecruitAI.Application.DTOs.Responses.Admin
{
	public class StatsResponseDto
	{
		public SummaryStatsDto Summary { get; set; } = new();
		public Dictionary<string, int> UsersByRole { get; set; } = new();
		public Dictionary<string, int> CVsByStatus { get; set; } = new();
		public Dictionary<string, int> JobsByStatus { get; set; } = new();
		public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();
		public RecentTrendDto RecentTrend { get; set; } = new();
		public FilterInfoDto? Filter { get; set; }
	}
	public class FilterInfoDto
	{
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
	}

	public class SummaryStatsDto
	{
		public int TotalCVs { get; set; }
		public int TotalJobs { get; set; }
		public int TotalUsers { get; set; }
		public int TotalApplications { get; set; }
	}

	public class RecentTrendDto
	{
		public int[] CVsLast7Days { get; set; } = new int[7];
		public int[] JobsLast7Days { get; set; } = new int[7];
		public int[] ApplicationsLast7Days { get; set; } = new int[7];
	}
}