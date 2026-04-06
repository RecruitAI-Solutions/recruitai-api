using MediatR;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Jobs;
using RecruitAI.Application.Interfaces;

namespace RecruitAI.Application.Queries.Jobs;

public class GetMyApplicationsQueryHandler : IRequestHandler<GetMyApplicationsQuery, PaginationResponseDto<MyApplicationDto>>
{
	private readonly IUnitOfWork _unitOfWork;

	public GetMyApplicationsQueryHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<PaginationResponseDto<MyApplicationDto>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
	{
		var result = await _unitOfWork.JobApplications.GetByUserIdAsync(
			request.UserId, request.Page, request.PageSize, request.Status, cancellationToken);

		var items = result.Items.Select(app => new MyApplicationDto
		{
			ApplicationId = app.Id,
			JobId = app.JobId,
			JobTitle = app.Job?.Title ?? "Unknown",
			Company = app.Job?.Department ?? "Unknown",
			Location = app.Job?.Location ?? "Unknown",
			MatchPercentage = app.Match?.MatchPercentage ?? 0,
			Status = app.Status,
			AppliedAt = app.AppliedAt,
			ReviewedAt = app.ReviewedAt
		}).ToList();

		return new PaginationResponseDto<MyApplicationDto>
		{
			Data = items,
			Total = result.Total,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}
}