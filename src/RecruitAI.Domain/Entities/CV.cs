using RecruitAI.Domain.Enums;
using System.Net.NetworkInformation;

namespace RecruitAI.Domain.Entities;

public class CV
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public string FileName { get; set; } = string.Empty;
	public string StoredFileName { get; set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;  // Đường dẫn tương đối
	public long FileSize { get; set; }
	public string ContentType { get; set; } = string.Empty;
	public CVStatus Status { get; set; }
	public DateTime UploadedAt { get; set; }
	public DateTime? ProcessedAt { get; set; }
	public string? ErrorMessage { get; set; }

	public virtual User User { get; set; } = null!;

}