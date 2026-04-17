using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Queries.Admin;

public class GetApplicationsByMonthReportQuery : IRequest<MonthlyApplicationReportDto>
{
	public int? Year { get; set; }
	public JobApplicationStatus? Status { get; set; }
}

public class GetApplicationsByMonthReportQueryHandler : IRequestHandler<GetApplicationsByMonthReportQuery, MonthlyApplicationReportDto>
{
	private readonly IUnitOfWork _uow;

	public GetApplicationsByMonthReportQueryHandler(IUnitOfWork uow)
	{
		_uow = uow;
	}

	public async Task<MonthlyApplicationReportDto> Handle(GetApplicationsByMonthReportQuery request, CancellationToken cancellationToken)
	{
		var year = request.Year ?? DateTime.UtcNow.Year;

		var stats = await _uow.JobApplications.GetApplicationsByMonthAsync(year, request.Status, cancellationToken);

		var data = new List<MonthlyApplicationDataDto>();
		var total = 0;

		for (int month = 1; month <= 12; month++)
		{
			var stat = stats.FirstOrDefault(s => s.Month == month);
			var count = stat?.Total ?? 0;
			total += count;

			data.Add(new MonthlyApplicationDataDto
			{
				Month = month,
				MonthName = GetMonthName(month),
				Total = count,
				Pending = stat?.Pending ?? 0,
				Reviewed = stat?.Reviewed ?? 0,
				Accepted = stat?.Accepted ?? 0,
				Rejected = stat?.Rejected ?? 0
			});
		}

		return new MonthlyApplicationReportDto
		{
			Year = year,
			Data = data,
			Total = total,
			Average = total / 12.0
		};
	}

	private string GetMonthName(int month)
	{
		return new DateTime(2000, month, 1).ToString("MMMM");
	}
}