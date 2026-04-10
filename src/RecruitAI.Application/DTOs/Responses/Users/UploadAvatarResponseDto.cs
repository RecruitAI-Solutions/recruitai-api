namespace RecruitAI.Application.DTOs.Responses.Users
{
	public class UploadAvatarResponseDto
	{
		public string AvatarUrl { get; set; } = string.Empty;
		public string ThumbnailUrl { get; set; } = string.Empty;
		public DateTime UpdatedAt { get; set; }
	}
}