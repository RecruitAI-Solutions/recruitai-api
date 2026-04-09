using RecruitAI.Domain.Enums;

namespace RecruitAI.Domain.Entities
{
	public class AuditLog
	{
		public Guid Id { get; set; }
		public AuditEntityType EntityType { get; set; }      // User, CV, Job, Application
		public AuditAction Action { get; set; }           // Create, Update, Delete, Login, ChangeStatus, ChangeRole, Apply, Analyze, Upload
		public string EntityId { get; set; }                          // ID của thực thể bị thay đổi
		public string EntityName { get; set; } = string.Empty;      // Tên hiển thị (email, title, filename)
		public string? OldValue { get; set; }                       // JSON giá trị cũ
		public string? NewValue { get; set; }                       // JSON giá trị mới
		public string? Reason { get; set; }                         // Lý do thay đổi
		public string ChangedBy { get; set; } = string.Empty;       // Email người thực hiện
		public string? ChangedByIp { get; set; }                    // IP người thực hiện
		public string? UserAgent { get; set; }                      // Trình duyệt/thiết bị
		public string? RequestId { get; set; }                      // Trace ID cho debug
		public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
	}
}