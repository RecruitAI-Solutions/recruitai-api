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

        if (!string.IsNullOrWhiteSpace(dto.EmploymentType))
        {
            if (Enum.TryParse<EmploymentType>(dto.EmploymentType, true, out var et))
                domain.EmploymentType = et;
        }

        if (!string.IsNullOrWhiteSpace(dto.ExperienceLevel))
        {
            if (Enum.TryParse<ExperienceLevel>(dto.ExperienceLevel, true, out var el))
                domain.ExperienceLevel = el;
        }

        return domain;
    }
}
