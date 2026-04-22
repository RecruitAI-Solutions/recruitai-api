using FluentValidation;
using Microsoft.AspNetCore.Http;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Application.DTOs.Requests.Auths;

public class UploadCVRequestDto
{
	/// <summary>
	/// File CV cần upload (chỉ chấp nhận PDF)
	/// </summary>
	public IFormFile File { get; set; } = null!;
}

public class UploadCVRequestValidator : AbstractValidator<UploadCVRequestDto>
{
	public UploadCVRequestValidator(IMessageService msg)
	{
		RuleFor(x => x.File)
			.NotNull().WithMessage(msg.Validation("FileRequired"))
			.Must(x => x.Length > 0).WithMessage(msg.Validation("FileEmpty"))
			.Must(x => x.Length <= 10 * 1024 * 1024).WithMessage(msg.Validation("FileTooLarge"))
			.Must(x => x.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
				.WithMessage(msg.Validation("OnlyPdfAllowed"));
	}
}