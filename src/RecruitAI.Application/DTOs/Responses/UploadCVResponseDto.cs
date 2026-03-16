namespace RecruitAI.Application.DTOs.Responses;

public class UploadCVResponseDto
{
	/// <summary>
	/// ID của CV vừa upload
	/// </summary>
	public Guid CvId { get; set; }

	/// <summary>
	/// Tên file gốc
	/// </summary>
	public string FileName { get; set; } = string.Empty;

	/// <summary>
	/// Đường dẫn tương đối đến file
	/// </summary>
	public string FilePath { get; set; } = string.Empty;

	/// <summary>
	/// Kích thước file (bytes)
	/// </summary>
	public long FileSize { get; set; }

	/// <summary>
	/// Thời gian upload
	/// </summary>
	public DateTime UploadedAt { get; set; }

	/// <summary>
	/// Trạng thái xử lý (Pending/Processing/Completed/Failed)
	/// </summary>
	public string Status { get; set; } = string.Empty;

	/// <summary>
	/// Link download file (nếu cần)
	/// </summary>
	public string? DownloadUrl { get; set; }
}