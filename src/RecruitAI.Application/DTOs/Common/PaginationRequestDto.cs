namespace RecruitAI.Application.DTOs.Common;

public class PaginationRequestDto
{
	private const int MaxPageSize = 50;
	private int _pageSize = 10;

	/// <summary>
	/// Số trang (bắt đầu từ 1)
	/// </summary>
	public int Page { get; set; } = 1;

	/// <summary>
	/// Số lượng item trên mỗi trang (tối đa 50)
	/// </summary>
	public int PageSize
	{
		get => _pageSize;
		set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
	}
}