using AutoMapper;
using RecruitAI.Application.Commands.Jobs;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Mappings;

public class JobProfile : Profile
{
	public JobProfile()
	{
		// Job -> DTOs
		CreateMap<Job, JobListDto>()
			.ForMember(dest => dest.RecruiterName,
				opt => opt.MapFrom(src => src.Recruiter.FullName));

		CreateMap<Job, JobDetailDto>()
			.ForMember(dest => dest.RecruiterName,
				opt => opt.MapFrom(src => src.Recruiter.FullName))
			.ForMember(dest => dest.RecruiterEmail,
				opt => opt.MapFrom(src => src.Recruiter.Email))
			.ForMember(dest => dest.Skills,
				opt => opt.MapFrom(src => src.GetSkillsList()));

		// Command -> Job
		CreateMap<CreateJobCommand, Job>()
			.ForMember(dest => dest.Skills,
				opt => opt.MapFrom(src => string.Join(",", src.Skills)))
			.ForMember(dest => dest.CreatedAt,
				opt => opt.MapFrom(src => DateTime.UtcNow))
			.ForMember(dest => dest.IsActive,
				opt => opt.MapFrom(src => true))
			.ForMember(dest => dest.Views,
				opt => opt.MapFrom(src => 0))
			.ForMember(dest => dest.Applications,
				opt => opt.MapFrom(src => 0));

		CreateMap<UpdateJobCommand, Job>()
			.ForMember(dest => dest.Skills,
				opt => opt.MapFrom(src => string.Join(",", src.Skills)))
			.ForMember(dest => dest.UpdatedAt,
				opt => opt.MapFrom(src => DateTime.UtcNow));
	}
}