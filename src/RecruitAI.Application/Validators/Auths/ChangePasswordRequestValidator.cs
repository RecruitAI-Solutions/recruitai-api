using FluentValidation;
using RecruitAI.Application.DTOs.Requests.Auths;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Application.Validators.Auths
{
	public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>
	{
		public ChangePasswordRequestValidator(IMessageService msg, IValidationService validationService)
		{
			RuleFor(x => x.CurrentPassword)
				.NotEmpty().WithMessage(msg.Validation("PasswordRequired"));

			RuleFor(x => x.NewPassword)
				.NotEmpty().WithMessage(msg.Validation("PasswordRequired"))
				.MinimumLength(6).WithMessage(msg.Validation("PasswordMinLength"))
				.Must(password => validationService.IsStrongPassword(password))
				.WithMessage(msg.Validation("PasswordTooWeak"))
				.NotEqual(x => x.CurrentPassword)
				.WithMessage(msg.Business("NewPasswordSameAsOld"));

			RuleFor(x => x.ConfirmNewPassword)
				.NotEmpty().WithMessage(msg.Validation("PasswordRequired"))
				.Equal(x => x.NewPassword).WithMessage(msg.Validation("PasswordMismatch"));
		}
	}
}