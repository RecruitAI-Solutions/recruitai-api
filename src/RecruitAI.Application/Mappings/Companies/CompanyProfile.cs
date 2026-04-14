using AutoMapper;
using RecruitAI.Application.DTOs.Responses.Companies;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Mappings.Companies;

public class CompanyProfile : Profile
{
	public CompanyProfile()
	{
		CreateMap<Company, CompanyResponseDto>()
			.ForMember(dest => dest.TotalJobs, opt => opt.Ignore());

		CreateMap<Company, CompanySuggestDto>();
	}
}