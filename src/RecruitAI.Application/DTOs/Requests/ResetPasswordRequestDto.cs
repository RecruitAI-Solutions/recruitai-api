using System.ComponentModel.DataAnnotations;

namespace RecruitAI.Application.DTOs.Requests
{
	public class ResetPasswordRequestDto
	{
		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email format")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Token is required")]
		public string Token { get; set; } = string.Empty;

		[Required(ErrorMessage = "New password is required")]
		[MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
		public string NewPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Please confirm your new password")]
		[Compare("NewPassword", ErrorMessage = "Passwords do not match")]
		public string ConfirmNewPassword { get; set; } = string.Empty;
	}
}