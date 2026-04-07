namespace RecruitAI.Application.DTOs.Responses.Auths
{
	public class ForgotPasswordResponseDto
	{
		/// <summary>
		/// Thành công hay không
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Thông báo cho người dùng
		/// </summary>
		public string Message { get; set; } = string.Empty;

		/// <summary>
		/// Thời gian xử lý
		/// </summary>
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
	}
}