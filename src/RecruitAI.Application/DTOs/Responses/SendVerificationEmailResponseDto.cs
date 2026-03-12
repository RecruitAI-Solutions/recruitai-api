namespace RecruitAI.Application.DTOs.Responses
{
	public class SendVerificationEmailResponseDto
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
	}
}