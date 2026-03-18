using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.CVs;

public class CVDetailDto
{
	public Guid Id { get; set; }
	public string FileName { get; set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;
	public long FileSize { get; set; }
	public string ContentType { get; set; } = string.Empty;
	public DateTime UploadedAt { get; set; }
	public DateTime? ProcessedAt { get; set; }
	public CVStatus Status { get; set; }
	public string? ErrorMessage { get; set; }
	public string DownloadUrl { get; set; } = string.Empty;
	public string FormattedFileSize => GetFormattedFileSize();

	private string GetFormattedFileSize()
	{
		string[] sizes = { "B", "KB", "MB", "GB" };
		double len = FileSize;
		int order = 0;
		while (len >= 1024 && order < sizes.Length - 1)
		{
			order++;
			len = len / 1024;
		}
		return $"{len:0.##} {sizes[order]}";
	}
}