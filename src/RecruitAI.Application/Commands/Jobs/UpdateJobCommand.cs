using MediatR;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Commands.Jobs;

public class UpdateJobCommand : IRequest<JobDetailDto>
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Requirements { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;

	// Lương
	public decimal? SalaryMin { get; set; }
	public decimal? SalaryMax { get; set; }
	public Currency Currency { get; set; } = Currency.VND;

	// Loại hình
	public EmploymentType EmploymentType { get; set; }
	public ExperienceLevel ExperienceLevel { get; set; }
	public string Department { get; set; } = string.Empty;

	// Kỹ năng
	public List<string> Skills { get; set; } = new();
	public string Benefits { get; set; } = string.Empty;

	// Thời gian
	public DateTime ExpirationDate { get; set; }

	// Kiểm tra quyền sở hữu
	public Guid RecruiterId { get; set; }
}