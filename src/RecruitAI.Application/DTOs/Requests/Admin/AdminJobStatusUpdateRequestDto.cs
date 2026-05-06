using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Requests.Admin
{
	public class AdminJobStatusUpdateRequestDto
	{
		/// <summary>
		/// Trạng thái mới (1:Draft, 2:Published, 3:Closed)
		/// </summary>
		public JobStatus Status { get; set; }

		/// <summary>
		/// Lý do thay đổi (tùy chọn)
		/// </summary>
		public string? Reason { get; set; }
	}

}
