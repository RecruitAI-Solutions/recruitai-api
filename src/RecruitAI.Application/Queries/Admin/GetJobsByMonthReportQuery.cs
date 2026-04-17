using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Queries.Admin;

public class GetJobsByMonthReportQuery : IRequest<MonthlyReportDto>
{
	public int? Year { get; set; }
}

public class GetJobsByMonthReportQueryHandler : IRequestHandler<GetJobsByMonthReportQuery, MonthlyReportDto>
{
	private readonly IUnitOfWork _uow;

	public GetJobsByMonthReportQueryHandler(IUnitOfWork uow)
	{
		_uow = uow;
	}

	public async Task<MonthlyReportDto> Handle(GetJobsByMonthReportQuery request, CancellationToken cancellationToken)
	{
		var year = request.Year ?? DateTime.UtcNow.Year;
		var stats = await _uow.Jobs.GetJobsByMonthAsync(year, cancellationToken);

		var data = new List<MonthlyDataDto>();
		var total = 0;

		for (int month = 1; month <= 12; month++)
		{
			var monthData = stats.FirstOrDefault(j => j.Month == month);
			var count = monthData?.Total ?? 0;
			total += count;

			data.Add(new MonthlyDataDto
			{
				Month = month,
				MonthName = GetMonthName(month),
				Total = count
			});
		}

		return new MonthlyReportDto
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