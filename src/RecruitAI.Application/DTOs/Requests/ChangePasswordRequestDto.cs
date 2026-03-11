using System.ComponentModel.DataAnnotations;

namespace RecruitAI.Application.DTOs.Requests
{
	public class ChangePasswordRequestDto
	{
		[Required(ErrorMessage = "ValidationPasswordRequired")]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "ValidationPasswordRequired")]
		[MinLength(6, ErrorMessage = "ValidationPasswordMinLength")]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "ValidationPasswordRequired")]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "ValidationPasswordMismatch")]
		public string ConfirmNewPassword { get; set; } = string.Empty;
	}
}