using RecruitAI.Domain.Enums;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Domain.Common.Skills;

namespace RecruitAI.Application.DTOs.Jobs;

public class JobDetailDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Requirements { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;

	// Lương
	public decimal? SalaryMin { get; set; }
	public decimal? SalaryMax { get; set; }
	public Currency Currency { get; set; }

	// Loại hình
	public EmploymentType EmploymentType { get; set; }
	public ExperienceLevel ExperienceLevel { get; set; }
	public string Department { get; set; } = string.Empty;

	// Enum friendly names
	public string CurrencyName { get; set; } = string.Empty;
	public string EmploymentTypeName { get; set; } = string.Empty;
	public string ExperienceLevelName { get; set; } = string.Empty;

	// Kỹ năng
	public List<int> SkillIds { get; set; } = new();
	public List<SkillMapping> SkillDetails { get; set; } = new();

	// Phúc lợi
	public string Benefits { get; set; } = string.Empty;

	// Thời gian
	public DateTime ExpirationDate { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	// Trạng thái
	public JobStatus Status { get; set; }
	public string StatusName { get; set; } = string.Empty;
	public bool IsActive { get; set; }

	// Thông tin Recruiter
	public Guid RecruiterId { get; set; }
	public string RecruiterName { get; set; } = string.Empty;
	public string RecruiterEmail { get; set; } = string.Empty;

	// Company info
	public Guid? CompanyId { get; set; }
	public string? CompanyName { get; set; }
	public string? CompanyLogo { get; set; }
	public string? CompanyWebsite { get; set; }
	public string? CompanyAddress { get; set; }

	// Thống kê
	public int Views { get; set; }
	public int Applications { get; set; }
}