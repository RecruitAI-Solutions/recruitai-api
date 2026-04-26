using RecruitAI.Domain.Enums;
using System.Text.Json.Serialization;

namespace RecruitAI.Application.DTOs.Responses.Auths
{
	public class AuthResponseDto
	{
		// Token chính
		public string AccessToken { get; set; }

		// Thêm refresh token (quan trọng!)
		public string RefreshToken { get; set; }

		// Thông tin user cơ bản
		public string UserId { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }

		// Thời gian hết hạn (seconds)
		public int ExpiresIn { get; set; }

		// Loại token (Bearer)
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string TokenType { get; set; } = "Bearer";

		// Roles/permissions
		public int Role { get; set; }
		public string RoleName { get; set; } = string.Empty; // Tên hiển thị theo ngôn ngữ
		// Permissions
		public List<string> Permissions { get; set; } = new(); // Danh sách mã permission
		public string? AvatarUrl { get; set; }

		// Constructor để khởi tạo nhanh
		public AuthResponseDto()
		{
			TokenType = "Bearer";
		}

		// Factory method để tạo response chuẩn
		public static AuthResponseDto Create(string accessToken, string refreshToken, Domain.Entities.User user, int expiresIn = 900)
		{
			return new AuthResponseDto
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				UserId = user.Id.ToString(),
				Email = user.Email,
				FullName = user.FullName,
				ExpiresIn = expiresIn,
				TokenType = "Bearer"
			};
		}
	}
}