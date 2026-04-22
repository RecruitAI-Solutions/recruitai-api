using AutoMapper;
using RecruitAI.Application.DTOs.CVs;
using RecruitAI.Domain.Common.CVs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecruitAI.Application.Mappings.CVs
{
	internal class MappingCVProfileWithTotalSkills : Profile
	{
		public MappingCVProfileWithTotalSkills()
		{
			// Mapping cho CVList -> CVListDto
			CreateMap<CVList, CVListDto>()
				.ForMember(dest => dest.TotalSkills,
					opt => opt.MapFrom(src => src.TotalSkills))
				.ForMember(dest => dest.FormattedFileSize,
					opt => opt.MapFrom(src => FormatFileSize(src.FileSize)));
		}

		private string FormatFileSize(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			double len = bytes;
			int order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				order++;
				len = len / 1024;
			}
			return $"{len:0.##} {sizes[order]}";
		}

	}
}
