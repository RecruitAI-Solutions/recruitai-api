using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Entities;
public class Company
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Slug { get; set; }
	public string? Logo { get; set; }
	public string? Address { get; set; }
	public string? Website { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public Guid CreatedBy { get; set; }

	public virtual User Creator { get; set; } = null!;
	public virtual ICollection<Job> Jobs { get; set; } = new HashSet<Job>();
}