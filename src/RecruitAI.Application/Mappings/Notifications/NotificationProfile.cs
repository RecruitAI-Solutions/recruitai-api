using AutoMapper;
using RecruitAI.Application.DTOs.Responses.Notifications;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Application.Mappings.Notifications;

public class NotificationProfile : Profile
{
	public NotificationProfile()
	{
		CreateMap<Notification, NotificationResponseDto>()
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
			.ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
			.ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
			.ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
			.ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead))
			.ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data))
			.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
	}
}