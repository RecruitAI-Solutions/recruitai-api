using AutoMapper;
using RecruitAI.Application.Commands.Jobs;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Mappings;

public class JobProfile : Profile
{
	public JobProfile()
	{
		// ===== JOB -> DTO =====

		// Job -> JobListDto
		CreateMap<Job, JobListDto>()
			.ForMember(dest => dest.RecruiterName,
				opt => opt.MapFrom(src => src.Recruiter != null ? src.Recruiter.FullName : string.Empty))
			.ForMember(dest => dest.SkillNames,
				opt => opt.MapFrom(src => src.JobSkills != null
					? src.JobSkills.Where(js => js.Skill != null).Select(js => js.Skill.Name).ToList()
					: new List<string>()));

		// Job -> JobDetailDto
		CreateMap<Job, JobDetailDto>()
			.ForMember(dest => dest.RecruiterName,
				opt => opt.MapFrom(src => src.Recruiter != null ? src.Recruiter.FullName : string.Empty))
			.ForMember(dest => dest.RecruiterEmail,
				opt => opt.MapFrom(src => src.Recruiter != null ? src.Recruiter.Email : string.Empty))
			// Map từ JobSkills sang SkillIds
			.ForMember(dest => dest.SkillIds,
				opt => opt.MapFrom(src => src.JobSkills != null
					? src.JobSkills.Select(js => js.SkillId).ToList()
					: new List<int>()))
			// Map chi tiết skill (không dùng ?. trong lambda)
			.ForMember(dest => dest.SkillDetails,
				opt => opt.MapFrom(src => src.JobSkills != null && src.JobSkills.Any()
					? src.JobSkills
						.Where(js => js.Skill != null)
						.Select(js => new SkillDto
						{
							Id = js.SkillId,
							Name = js.Skill != null ? js.Skill.Name : string.Empty,
							Category = js.Skill != null ? js.Skill.Category : null,
							IsRequired = js.IsRequired
						}).ToList()
					: new List<SkillDto>()));

		// ===== COMMAND -> JOB =====

		// CreateJobCommand -> Job
		CreateMap<CreateJobCommand, Job>()
			.ForMember(dest => dest.JobSkills, opt => opt.Ignore())
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.Views, opt => opt.Ignore())
			.ForMember(dest => dest.Applications, opt => opt.Ignore())
			.ForMember(dest => dest.Recruiter, opt => opt.Ignore());

		// UpdateJobCommand -> Job
		CreateMap<UpdateJobCommand, Job>()
			.ForMember(dest => dest.JobSkills, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.Recruiter, opt => opt.Ignore())
			.ForMember(dest => dest.Views, opt => opt.Ignore())
			.ForMember(dest => dest.Applications, opt => opt.Ignore());
	}
}