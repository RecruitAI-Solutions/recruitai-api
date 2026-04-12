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
	/// Danh sách kỹ năng để lọc (support multiple). If MatchAllSkills is true, job must contain all skills; otherwise any match.
	/// Binds from query string as repeated parameters: ?skills=java&skills=csharp
	/// </summary>
	public List<string>? Skills { get; set; }

	/// <summary>
	/// Nếu true thì job phải có tất cả các kỹ năng trong Skills (AND). Nếu false (mặc định) thì bất kỳ kỹ năng trùng khớp sẽ được chấp nhận (OR).
	/// </summary>
	public bool MatchAllSkills { get; set; } = false;

	/// <summary>
	/// Sắp xếp theo trường nào (createdAt, salary, title)
	/// </summary>
	public string? SortBy { get; set; } = "createdAt";

	/// <summary>
	/// Thứ tự sắp xếp (asc, desc)
	/// </summary>
	public string SortOrder { get; set; } = "desc";
}