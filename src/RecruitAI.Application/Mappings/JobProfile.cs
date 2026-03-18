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
				opt => opt.MapFrom(src => src.Recruiter.FullName))
			.ForMember(dest => dest.SkillNames,
				opt => opt.MapFrom(src => src.JobSkills.Select(js => js.Skill.Name).ToList()));

		// Job -> JobDetailDto
		CreateMap<Job, JobDetailDto>()
			.ForMember(dest => dest.RecruiterName,
				opt => opt.MapFrom(src => src.Recruiter.FullName))
			.ForMember(dest => dest.RecruiterEmail,
				opt => opt.MapFrom(src => src.Recruiter.Email))
			// Map từ JobSkills sang SkillIds
			.ForMember(dest => dest.SkillIds,
				opt => opt.MapFrom(src => src.JobSkills.Select(js => js.SkillId).ToList()))
			// Map chi tiết skill
			.ForMember(dest => dest.SkillDetails,
				opt => opt.MapFrom(src => src.JobSkills.Select(js => new SkillDto
				{
					Id = js.SkillId,
					Name = js.Skill.Name,
					Category = js.Skill.Category,
					IsRequired = js.IsRequired
				}).ToList()));

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