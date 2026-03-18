using FluentValidation;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Commands.CVs;

namespace RecruitAI.Application.Validators;

public class UploadCVCommandValidator : AbstractValidator<UploadCVCommand>
{
	private readonly IMessageService _msg;

	public UploadCVCommandValidator(IMessageService messageService)
	{
		_msg = messageService;

		RuleFor(x => x.FileStream)
			.NotNull()
			.WithMessage(_msg.Validation("FileRequired")); // "File không được để trống"

		RuleFor(x => x.FileName)
			.NotEmpty()
			.WithMessage(_msg.Validation("FileNameRequired")); // "Tên file không được để trống"

		RuleFor(x => x.FileSize)
			.GreaterThan(0)
			.WithMessage(_msg.Validation("FileEmpty")); // "File rỗng"

		RuleFor(x => x.ContentType)
			.NotEmpty()
			.Must(x => x.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
			.WithMessage(_msg.Validation("OnlyPdfAllowed")); // "Chỉ chấp nhận file PDF"
	}
}