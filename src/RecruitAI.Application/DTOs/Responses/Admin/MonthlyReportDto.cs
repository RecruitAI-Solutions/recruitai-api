namespace RecruitAI.Application.DTOs.Responses.Admin;

public class MonthlyReportDto
{
	public int Year { get; set; }
	public List<MonthlyDataDto> Data { get; set; } = new();
	public int Total { get; set; }
	public double Average { get; set; }
}

public class MonthlyDataDto
{
	public int Month { get; set; }
	public string MonthName { get; set; } = string.Empty;
	public int Total { get; set; }
}

public class MonthlyApplicationReportDto
{
	public int Year { get; set; }
	public List<MonthlyApplicationDataDto> Data { get; set; } = new();
	public int Total { get; set; }
	public double Average { get; set; }
}

public class MonthlyApplicationDataDto : MonthlyDataDto
{
	public int Pending { get; set; }
	public int Reviewed { get; set; }
	public int Accepted { get; set; }
	public int Rejected { get; set; }
}