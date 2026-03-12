using System.ComponentModel.DataAnnotations;

namespace RecruitAI.Application.DTOs.Requests
{
	public class SendVerificationEmailRequestDto
	{
		[Required(ErrorMessage = "ValidationEmailRequired")]
		[EmailAddress(ErrorMessage = "ValidationEmailInvalid")]
		public string Email { get; set; } = string.Empty;
	}
}