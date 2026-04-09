using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetAdminStatsQuery : IRequest<StatsResponseDto>
	{
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
	}
}