using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Common.Skills;	

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

	// Kỹ năng - Dùng bảng trung gian thay vì string
	public virtual ICollection<JobSkill> JobSkills { get; set; } = new HashSet<JobSkill>();

	// Phúc lợi
	public string Benefits { get; set; } = string.Empty;

	// Thời gian
	public DateTime ExpirationDate { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	// Trạng thái
	public JobStatus Status { get; set; } = JobStatus.Draft;
	public bool IsDeleted { get; set; } = false;
	public bool IsActive => Status == JobStatus.Published && !IsDeleted && ExpirationDate > DateTime.UtcNow;

	public void SetActive(bool active)
	{
		if (active && !IsActive)
		{
			Status = JobStatus.Published;
			UpdatedAt = DateTime.UtcNow;
		}
		else if (!active && IsActive)
		{
			Status = JobStatus.Closed;
			UpdatedAt = DateTime.UtcNow;
		}
	}

	// Thống kê
	public int Views { get; set; }
	public int Applications { get; set; }

	public Guid? CompanyId { get; set; }
	public virtual Company? Company { get; set; }

	public bool IsFeatured { get; set; } = false;  // Công việc nổi bật
	public int? FeaturedOrder { get; set; } // Thứ tự hiển thị

	// ===== HELPER METHODS MỚI =====

	/// <summary>
	/// Lấy danh sách ID của các skill
	/// </summary>
	public List<int> GetSkillIds()
	{
		return JobSkills?.Select(js => js.SkillId).ToList() ?? new List<int>();
	}

	/// <summary>
	/// Lấy danh sách tên skill
	/// </summary>
	public List<string> GetSkillNames()
	{
		return JobSkills?
			.Where(js => js.Skill != null)
			.Select(js => js.Skill.Name)
			.ToList() ?? new List<string>();
	}

	/// <summary>
	/// Lấy danh sách skill kèm theo IsRequired
	/// </summary>
	public List<SkillMapping> GetSkillDetails()
	{
		return JobSkills?
			.Where(js => js.Skill != null)
			.Select(js => new SkillMapping
			{
				Id = js.SkillId,
				Name = js.Skill!.Name,
				Category = js.Skill.Category,
				IsRequired = js.IsRequired
			})
			.ToList() ?? new List<SkillMapping>();
	}


	/// <summary>
	/// Thêm skill vào job
	/// </summary>
	public void AddSkill(int skillId, bool isRequired = true)
	{
		if (JobSkills == null)
			JobSkills = new HashSet<JobSkill>();

		if (!JobSkills.Any(js => js.SkillId == skillId))
		{
			JobSkills.Add(new JobSkill
			{
				JobId = this.Id,
				SkillId = skillId,
				IsRequired = isRequired
			});
		}
	}

	/// <summary>
	/// Thêm nhiều skill cùng lúc
	/// </summary>
	public void AddSkills(List<int> skillIds, bool isRequired = true)
	{
		foreach (var skillId in skillIds)
		{
			AddSkill(skillId, isRequired);
		}
	}

	/// <summary>
	/// Xóa skill khỏi job
	/// </summary>
	public void RemoveSkill(int skillId)
	{
		var skill = JobSkills?.FirstOrDefault(js => js.SkillId == skillId);
		if (skill != null)
		{
			JobSkills.Remove(skill);
		}
	}

	/// <summary>
	/// Cập nhật danh sách skill (xóa cũ, thêm mới)
	/// </summary>
	public void UpdateSkills(List<int> newSkillIds, bool isRequired = true)
	{
		JobSkills?.Clear();
		AddSkills(newSkillIds, isRequired);
	}

	/// <summary>
	/// Kiểm tra job có skill không
	/// </summary>
	public bool HasSkills()
	{
		return JobSkills != null && JobSkills.Any();
	}

	/// <summary>
	/// Kiểm tra job có skill cụ thể không
	/// </summary>
	public bool HasSkill(int skillId)
	{
		return JobSkills != null && JobSkills.Any(js => js.SkillId == skillId);
	}

	/// <summary>
	/// Đếm số lượng skill
	/// </summary>
	public int SkillCount()
	{
		return JobSkills?.Count ?? 0;
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