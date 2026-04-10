using FluentValidation;
using RecruitAI.Application.DTOs.Requests.Auths;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Application.Validators.Auths
{
	public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
	{
		public ForgotPasswordRequestValidator(IMessageService msg, IValidationService validationService)
		{
			RuleFor(x => x.Email)
			.MustAsync(async (email, ct) => !await validationService.IsEmailUniqueAsync(email, ct))
			.WithMessage(msg.Business("UserNotFound"));

			// Kiểm tra email đã tồn tại
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage(msg.Validation("EmailRequired"))
				.EmailAddress().WithMessage(msg.Validation("EmailInvalid"))
				.MustAsync(async (email, ct) => !await validationService.IsEmailUniqueAsync(email, ct))
				.WithMessage(msg.Business("UserNotFound"));
		}
	}
}