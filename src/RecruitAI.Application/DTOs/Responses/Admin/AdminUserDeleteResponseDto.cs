namespace RecruitAI.Application.DTOs.Responses.Admin
{
	public class AdminUserDeleteResponseDto
	{
		public Guid Id { get; set; }
		public bool Deleted { get; set; }
		public DateTime DeletedAt { get; set; }
	}
}