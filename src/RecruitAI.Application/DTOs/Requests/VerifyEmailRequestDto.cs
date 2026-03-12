using System.ComponentModel.DataAnnotations;

namespace RecruitAI.Application.DTOs.Requests
{
	public class VerifyEmailRequestDto
	{
		[Required(ErrorMessage = "ValidationEmailRequired")]
		[EmailAddress(ErrorMessage = "ValidationEmailInvalid")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "ValidationTokenRequired")]
		public string Token { get; set; } = string.Empty;
	}
}