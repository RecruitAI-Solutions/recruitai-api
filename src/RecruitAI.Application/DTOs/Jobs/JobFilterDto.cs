using RecruitAI.Application.DTOs.Common;

namespace RecruitAI.Application.DTOs.Jobs;

public class JobFilterDto : PaginationRequestDto
{
	/// <summary>
	/// Tìm kiếm theo tiêu đề
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// Lọc theo địa điểm
	/// </summary>
	public string? Location { get; set; }

	/// <summary>
	/// Lương tối thiểu
	/// </summary>
	public decimal? MinSalary { get; set; }

	/// <summary>
	/// Lương tối đa
	/// </summary>
	public decimal? MaxSalary { get; set; }

	/// <summary>
	/// Loại hình công việc (Full-time, Part-time, Remote)
	/// </summary>
	public string? EmploymentType { get; set; }

	/// <summary>
	/// Cấp độ kinh nghiệm (Entry, Junior, Senior, Lead)
	/// </summary>
	public string? ExperienceLevel { get; set; }

	/// <summary>
	/// Kỹ năng (tìm kiếm chứa)
	/// </summary>
	public string? Skill { get; set; }

	/// <summary>
	/// Sắp xếp theo trường nào (createdAt, salary, title)
	/// </summary>
	public string? SortBy { get; set; } = "createdAt";

	/// <summary>
	/// Thứ tự sắp xếp (asc, desc)
	/// </summary>
	public string SortOrder { get; set; } = "desc";
}