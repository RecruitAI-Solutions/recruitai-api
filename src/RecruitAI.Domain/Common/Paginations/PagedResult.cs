namespace RecruitAI.Domain.Common.Paginations;

public class PagedResult<T>
{
	public List<T> Items { get; set; } = new();
	public int Total { get; set; }
}