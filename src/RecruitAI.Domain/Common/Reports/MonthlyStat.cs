// RecruitAI.Domain/Common/Reports/MonthlyStat.cs
namespace RecruitAI.Domain.Common.Reports;

public class MonthlyJobStat
{
	public int Month { get; set; }
	public int Total { get; set; }
}

public class MonthlyApplicationStat
{
	public int Month { get; set; }
	public int Total { get; set; }
	public int Pending { get; set; }
	public int Reviewed { get; set; }
	public int Accepted { get; set; }
	public int Rejected { get; set; }
}