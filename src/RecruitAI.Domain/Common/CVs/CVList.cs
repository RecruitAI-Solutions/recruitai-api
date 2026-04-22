using RecruitAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecruitAI.Domain.Common.CVs
{
	public class CVList
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string FileName { get; set; } = string.Empty;
		public string StoredFileName { get; set; } = string.Empty;
		public string FilePath { get; set; } = string.Empty;
		public long FileSize { get; set; }
		public string ContentType { get; set; } = string.Empty;
		public CVStatus Status { get; set; }
		public DateTime UploadedAt { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string? ErrorMessage { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime? DeletedAt { get; set; }
		public string? ExtractedText { get; set; }
		public DateTime? AnalyzedAt { get; set; }
		public int TotalSkills { get; set; }
		public string FormattedFileSize => FormatFileSize(FileSize);

		private string FormatFileSize(long fileSize)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			double len = fileSize;
			int order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				order++;
				len = len / 1024;
			}
			return $"{len:0.##} {sizes[order]}";
		}
	}
}
