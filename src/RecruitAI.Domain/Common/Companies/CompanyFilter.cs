namespace RecruitAI.Domain.Common.Companies;

public class CompanyFilter
{
	public string? Keyword { get; set; }
	public string? SortBy { get; set; } = "name";
	public string SortOrder { get; set; } = "asc";
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
}