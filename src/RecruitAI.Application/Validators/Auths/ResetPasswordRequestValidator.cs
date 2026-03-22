using FluentValidation;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Application.Validators.Auths
{
	public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
	{
		public ResetPasswordRequestValidator(IMessageService msg, IValidationService validationService)
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage(msg.Validation("EmailRequired"))
				.EmailAddress().WithMessage(msg.Validation("EmailInvalid"));

			RuleFor(x => x.Token)
				.NotEmpty().WithMessage(msg.Validation("TokenRequired"));

			RuleFor(x => x.NewPassword)
				.NotEmpty().WithMessage(msg.Validation("PasswordRequired"))
				.MinimumLength(6).WithMessage(msg.Validation("PasswordMinLength"))
				.Must(password => validationService.IsStrongPassword(password))
				.WithMessage(msg.Validation("PasswordTooWeak"));

			RuleFor(x => x.ConfirmNewPassword)
				.NotEmpty().WithMessage(msg.Validation("PasswordRequired"))
				.Equal(x => x.NewPassword).WithMessage(msg.Validation("PasswordMismatch"));
		}
	}
}