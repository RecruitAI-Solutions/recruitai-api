namespace RecruitAI.Application.DTOs.Responses.Admin
{
	public class AuditLogDetailResponseDto
	{
		public Guid Id { get; set; }
		public string EntityType { get; set; } = string.Empty;
		public string Action { get; set; } = string.Empty;
		public Guid EntityId { get; set; }
		public string EntityName { get; set; } = string.Empty;
		public string? OldValue { get; set; }
		public string? NewValue { get; set; }
		public string? Reason { get; set; }
		public string ChangedBy { get; set; } = string.Empty;
		public string? ChangedByIp { get; set; }
		public string? UserAgent { get; set; }
		public string? RequestId { get; set; }
		public DateTime ChangedAt { get; set; }
	}
}