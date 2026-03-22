using AutoMapper;
using RecruitAI.Application.DTOs.CVs;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Mappings.CVs;

public class CVProfile : Profile
{
	public CVProfile()
	{
		CreateMap<CV, CVListDto>();
		CreateMap<CV, CVDetailDto>()
			.ForMember(dest => dest.DownloadUrl,
				opt => opt.MapFrom(src => $"/api/v1/CV/{src.Id}"));
	}
}