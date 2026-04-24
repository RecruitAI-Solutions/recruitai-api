// RecruitAI.Shared/DTOs/CandidateDashboardDto.cs
namespace RecruitAI.Shared.DTOs
{
	public class CandidateDashboardDto
	{
		/// <summary>
		/// Số việc làm mới trong hôm nay
		/// </summary>
		public int NewJobsToday { get; set; }

		/// <summary>
		/// Tổng số đơn đã ứng tuyển
		/// </summary>
		public int TotalApplications { get; set; }

		/// <summary>
		/// Số công việc gợi ý phù hợp (dựa trên CV gần nhất)
		/// </summary>
		public int SuggestedJobs { get; set; }

		/// <summary>
		/// Số đơn ứng tuyển đã được review/xem
		/// </summary>
		public int ReviewedApplications { get; set; }

		/// <summary>
		/// Số CV đã phân tích
		/// </summary>
		public int AnalyzedCVs { get; set; }

		/// <summary>
		/// Số công việc đã lưu
		/// </summary>
		public int SavedJobs { get; set; }

		/// <summary>
		/// Số thông báo chưa đọc
		/// </summary>
		public int UnreadNotifications { get; set; }
	}
}