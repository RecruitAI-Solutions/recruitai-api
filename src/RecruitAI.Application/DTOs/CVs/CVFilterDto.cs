using RecruitAI.Application.DTOs.Common;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.CVs;

public class CVFilterDto : PaginationRequestDto
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
}