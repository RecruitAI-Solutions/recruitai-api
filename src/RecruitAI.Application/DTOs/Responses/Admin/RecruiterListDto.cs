// RecruiterListDto.cs
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses.Admin;

public class RecruiterListDto
{
	public Guid Id { get; set; }
	public string Email { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string? PhoneNumber { get; set; }
	public string? AvatarUrl { get; set; }
	public UserStatus Status { get; set; }
	public string StatusName { get; set; } = string.Empty;
	public int JobCount { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? LastLoginAt { get; set; }
}