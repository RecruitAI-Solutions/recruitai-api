using MediatR;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Interfaces;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.Queries.Admin
{
	public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, StatsResponseDto>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetAdminStatsQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<StatsResponseDto> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
		{
			// Xác định khoảng thời gian
			var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30); // Mặc định 30 ngày
			var toDate = request.ToDate ?? DateTime.UtcNow;

			// Đảm bảo toDate là cuối ngày
			var toDateEnd = toDate.Date.AddDays(1).AddTicks(-1);

			// Tổng quan (có filter thời gian)
			var totalCVs = await _unitOfWork.CVs.CountAsync(c => c.UploadedAt >= fromDate && c.UploadedAt <= toDateEnd, cancellationToken);
			var totalJobs = await _unitOfWork.Jobs.CountAsync(j => !j.IsDeleted && j.CreatedAt >= fromDate && j.CreatedAt <= toDateEnd, cancellationToken);
			var totalUsers = await _unitOfWork.Users.CountAsync(u => u.CreatedAt >= fromDate && u.CreatedAt <= toDateEnd, cancellationToken);
			var totalApplications = await _unitOfWork.JobApplications.CountAsync(a => a.AppliedAt >= fromDate && a.AppliedAt <= toDateEnd, cancellationToken);

			// Users by role (có filter thời gian)
			var usersByRole = await _unitOfWork.Users.CountUsersByRoleAsync(fromDate, toDateEnd, cancellationToken);

			// CVs by status (có filter thời gian)
			var cvsByStatus = await _unitOfWork.CVs.CountCVsByStatusAsync(fromDate, toDateEnd, cancellationToken);

			// Jobs by status (có filter thời gian)
			var jobsByStatus = await _unitOfWork.Jobs.CountJobsByStatusAsync(fromDate, toDateEnd, cancellationToken);

			// Applications by status (có filter thời gian)
			var appsByStatus = await _unitOfWork.JobApplications.CountApplicationsByStatusAsync(fromDate, toDateEnd, cancellationToken);

			// Recent trends (7 ngày gần nhất tính từ toDate)
			var cvsLast7Days = await _unitOfWork.CVs.CountCVsByDayAsync(7, toDate, cancellationToken);
			var jobsLast7Days = await _unitOfWork.Jobs.CountJobsByDayAsync(7, toDate, cancellationToken);
			var appsLast7Days = await _unitOfWork.JobApplications.CountApplicationsByDayAsync(7, toDate, cancellationToken);

			return new StatsResponseDto
			{
				Summary = new SummaryStatsDto
				{
					TotalCVs = totalCVs,
					TotalJobs = totalJobs,
					TotalUsers = totalUsers,
					TotalApplications = totalApplications
				},
				UsersByRole = usersByRole.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
				CVsByStatus = cvsByStatus.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
				JobsByStatus = jobsByStatus.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
				ApplicationsByStatus = appsByStatus.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
				RecentTrend = new RecentTrendDto
				{
					CVsLast7Days = cvsLast7Days,
					JobsLast7Days = jobsLast7Days,
					ApplicationsLast7Days = appsLast7Days
				},
				Filter = new FilterInfoDto
				{
					FromDate = fromDate,
					ToDate = toDate
				}
			};
		}
	}
}