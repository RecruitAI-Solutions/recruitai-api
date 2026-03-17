using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Common;

public class CVFilter
{
	/// <summary>
	/// Lọc theo trạng thái CV
	/// </summary>
	public CVStatus? Status { get; set; }

	/// <summary>
	/// Lọc từ ngày
	/// </summary>
	public DateTime? FromDate { get; set; }

	/// <summary>
	/// Lọc đến ngày
	/// </summary>
	public DateTime? ToDate { get; set; }

	/// <summary>
	/// Tìm kiếm theo tên file
	/// </summary>
	public string? FileName { get; set; }

	/// <summary>
	/// Sắp xếp theo trường (uploadedAt, fileName, fileSize, status)
	/// </summary>
	public string? SortBy { get; set; } = "uploadedAt";

	/// <summary>
	/// Thứ tự sắp xếp (asc, desc)
	/// </summary>
	public string SortOrder { get; set; } = "desc";

	/// <summary>
	/// Số trang
	/// </summary>
	public int Page { get; set; } = 1;

	/// <summary>
	/// Kích thước trang
	/// </summary>
	public int PageSize { get; set; } = 10;
}