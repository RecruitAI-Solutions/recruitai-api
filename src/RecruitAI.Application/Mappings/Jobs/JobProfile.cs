using AutoMapper;
using RecruitAI.Application.Commands.Jobs;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Domain.Common.Skills;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Mappings.Jobs;

public class JobProfile : Profile
{
	public JobProfile()
	{
		// ===== JOB -> JOBLISTDTO =====
		CreateMap<Job, JobListDto>()
			.ForMember(dest => dest.RecruiterName,
				opt => opt.MapFrom(src => src.Recruiter != null ? src.Recruiter.FullName : string.Empty))
			.ForMember(dest => dest.SkillNames,
				opt => opt.MapFrom(src => src.JobSkills != null
					? src.JobSkills.Where(js => js.Skill != null).Select(js => js.Skill.Name).ToList()
					: new List<string>()))
			.ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency.ToString()))
			.ForMember(dest => dest.EmploymentTypeName, opt => opt.MapFrom(src => src.EmploymentType.ToString()))
			.ForMember(dest => dest.ExperienceLevelName, opt => opt.MapFrom(src => src.ExperienceLevel.ToString()));

		// ===== JOB -> JOBDETAILDTO =====
		CreateMap<Job, JobDetailDto>()
			// Recruiter
			.ForMember(dest => dest.RecruiterName,
				opt => opt.MapFrom(src => src.Recruiter != null ? src.Recruiter.FullName : string.Empty))
			.ForMember(dest => dest.RecruiterEmail,
				opt => opt.MapFrom(src => src.Recruiter != null ? src.Recruiter.Email : string.Empty))
			// SkillIds
			.ForMember(dest => dest.SkillIds,
				opt => opt.MapFrom(src => src.JobSkills != null
					? src.JobSkills.Select(js => js.SkillId).ToList()
					: new List<int>()))
			// SkillDetails
			.ForMember(dest => dest.SkillDetails,
				opt => opt.MapFrom(src => src.JobSkills != null && src.JobSkills.Any()
					? src.JobSkills
						.Where(js => js.Skill != null)
						.Select(js => new SkillMapping
						{
							Id = js.SkillId,
							Name = js.Skill != null ? js.Skill.Name : string.Empty,
							Category = js.Skill != null ? js.Skill.Category : null,
							IsRequired = js.IsRequired
						}).ToList()
					: new List<SkillMapping>()))
			// Company
			.ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Company != null ? src.Company.Id : (Guid?)null))
			.ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null))
			.ForMember(dest => dest.CompanyLogo, opt => opt.MapFrom(src => src.Company != null ? src.Company.Logo : null))
			.ForMember(dest => dest.CompanyWebsite, opt => opt.MapFrom(src => src.Company != null ? src.Company.Website : null))
			.ForMember(dest => dest.CompanyAddress, opt => opt.MapFrom(src => src.Company != null ? src.Company.Address : null))
			// Enum names
			.ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency.ToString()))
			.ForMember(dest => dest.EmploymentTypeName, opt => opt.MapFrom(src => src.EmploymentType.ToString()))
			.ForMember(dest => dest.ExperienceLevelName, opt => opt.MapFrom(src => src.ExperienceLevel.ToString()));

		// ===== COMMAND -> JOB =====
		CreateMap<CreateJobCommand, Job>()
			.ForMember(dest => dest.JobSkills, opt => opt.Ignore())
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.Views, opt => opt.Ignore())
			.ForMember(dest => dest.Applications, opt => opt.Ignore())
			.ForMember(dest => dest.Recruiter, opt => opt.Ignore());

		CreateMap<UpdateJobCommand, Job>()
			.ForMember(dest => dest.JobSkills, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.Recruiter, opt => opt.Ignore())
			.ForMember(dest => dest.Views, opt => opt.Ignore())
			.ForMember(dest => dest.Applications, opt => opt.Ignore());
	}
}