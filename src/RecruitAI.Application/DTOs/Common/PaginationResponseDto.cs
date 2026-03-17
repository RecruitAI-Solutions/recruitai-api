namespace RecruitAI.Application.DTOs.Common;

public class PaginationResponseDto<T>
{
	/// <summary>
	/// Dữ liệu của trang hiện tại
	/// </summary>
	public List<T> Data { get; set; } = new();

	/// <summary>
	/// Tổng số bản ghi
	/// </summary>
	public int Total { get; set; }

	/// <summary>
	/// Trang hiện tại
	/// </summary>
	public int Page { get; set; }

	/// <summary>
	/// Số lượng bản ghi mỗi trang
	/// </summary>
	public int PageSize { get; set; }

	/// <summary>
	/// Tổng số trang
	/// </summary>
	public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);

	/// <summary>
	/// Có trang trước không?
	/// </summary>
	public bool HasPrevious => Page > 1;

	/// <summary>
	/// Có trang sau không?
	/// </summary>
	public bool HasNext => Page < TotalPages;
}