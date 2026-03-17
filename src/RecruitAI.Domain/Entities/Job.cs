using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Entities;

public class Job
{
	public Guid Id { get; set; }

	// Khóa ngoại đến User (Recruiter)
	public Guid RecruiterId { get; set; }
	public virtual User Recruiter { get; set; }

	// Thông tin cơ bản
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Requirements { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;

	// Lương
	public decimal? SalaryMin { get; set; }
	public decimal? SalaryMax { get; set; }
	public Currency Currency { get; set; } = Currency.VND;

	// Loại hình công việc
	public EmploymentType? EmploymentType { get; set; }

	// Cấp độ
	public ExperienceLevel? ExperienceLevel { get; set; }
	public string Department { get; set; } = string.Empty;

	// Kỹ năng (lưu dạng JSON hoặc CSV)
	public string Skills { get; set; } = string.Empty; // ".NET,React,SQL"
	public string Benefits { get; set; } = string.Empty;

	// Thời gian
	public DateTime ExpirationDate { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	// Trạng thái
	public JobStatus Status { get; set; } = JobStatus.Draft;
	public bool IsDeleted { get; set; } = false;
	public bool IsActive { get; set; } = true;

	// Thống kê
	public int Views { get; set; }
	public int Applications { get; set; }

	// Helper methods
	public List<string> GetSkillsList()
	{
		return string.IsNullOrEmpty(Skills)
			? new List<string>()
			: Skills.Split(',').Select(s => s.Trim()).ToList();
	}

	public void SetSkills(List<string> skills)
	{
		Skills = string.Join(",", skills);
	}

	public void MarkAsDeleted()
	{
		IsDeleted = true;
		UpdatedAt = DateTime.UtcNow;
	}

	public void IncrementViews()
	{
		Views++;
	}

	public bool IsExpired()
	{
		return ExpirationDate < DateTime.UtcNow;
	}

	public void Publish()
	{
		Status = JobStatus.Published;
		UpdatedAt = DateTime.UtcNow;
	}

	public void Close()
	{
		Status = JobStatus.Closed;
		UpdatedAt = DateTime.UtcNow;
	}
}