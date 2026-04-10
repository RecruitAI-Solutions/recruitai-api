namespace RecruitAI.Application.DTOs.Responses.Applications
{
	public class CvInfoDto
	{
		public Guid Id { get; set; }
		public string FileName { get; set; } = string.Empty;
		public long FileSize { get; set; }
		public DateTime UploadedAt { get; set; }
		public string DownloadUrl { get; set; } = string.Empty;
	}
}