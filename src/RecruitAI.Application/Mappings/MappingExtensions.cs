using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Domain.Common.Jobs;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Mappings;

public static class MappingExtensions
{
	public static JobFilter ToDomainFilter(this JobFilterDto dto)
	{
		var domain = new JobFilter
		{
			Title = dto.Title,
			Location = dto.Location,
			MinSalary = dto.MinSalary,
			MaxSalary = dto.MaxSalary,
			SortBy = dto.SortBy,
			SortOrder = dto.SortOrder,
			Page = dto.Page,
			PageSize = dto.PageSize,
			Skill = dto.Skill,
			Skills = dto.Skills,
			MatchAllSkills = dto.MatchAllSkills
		};

		// Chuyển đổi EmploymentType (List<int> -> List<EmploymentType>)
		if (dto.EmploymentType != null && dto.EmploymentType.Any())
		{
			domain.EmploymentType = dto.EmploymentType
				.Select(et => (EmploymentType)et)
				.ToList();
		}

		// Chuyển đổi ExperienceLevel (List<int> -> List<ExperienceLevel>)
		if (dto.ExperienceLevel != null && dto.ExperienceLevel.Any())
		{
			domain.ExperienceLevel = dto.ExperienceLevel
				.Select(el => (ExperienceLevel)el)
				.ToList();
		}

		return domain;
	}
}