using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetAuditLogDetailQuery : IRequest<AuditLogDetailResponseDto>
	{
		public Guid Id { get; set; }
	}
}