using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Domain.Common.Jobs;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Mappings;

public static class JobFilterMapping
{
	public static JobFilter ToDomainFilter(this JobFilterDto dto)
	{
		return new JobFilter
		{
			Title = dto.Title,
			Location = dto.Location,
			MinSalary = dto.MinSalary,
			MaxSalary = dto.MaxSalary,
			EmploymentType = !string.IsNullOrWhiteSpace(dto.EmploymentType)
				? Enum.Parse<EmploymentType>(dto.EmploymentType, true)
				: null,
			ExperienceLevel = !string.IsNullOrWhiteSpace(dto.ExperienceLevel)
				? Enum.Parse<ExperienceLevel>(dto.ExperienceLevel, true)
				: null,
			Skill = dto.Skill,
			SortBy = dto.SortBy,
			SortOrder = dto.SortOrder,
			Page = dto.Page,
			PageSize = dto.PageSize
		};
	}
}