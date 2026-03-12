using System.ComponentModel.DataAnnotations;

public class ResetPasswordRequestDto
{
	[Required(ErrorMessage = "ValidationEmailRequired")]
	[EmailAddress(ErrorMessage = "ValidationEmailInvalid")]
	public string Email { get; set; }

	[Required(ErrorMessage = "ValidationTokenRequired")]
	public string Token { get; set; }

	[Required(ErrorMessage = "ValidationPasswordRequired")]
	[MinLength(6, ErrorMessage = "ValidationPasswordMinLength")]
	public string NewPassword { get; set; }

	[Required(ErrorMessage = "ValidationPasswordRequired")]
	[Compare("NewPassword", ErrorMessage = "ValidationPasswordMismatch")]
	public string ConfirmNewPassword { get; set; }
}