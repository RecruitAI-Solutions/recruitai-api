using FluentValidation;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Services;

namespace RecruitAI.Application.Validators
{
	public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
	{
		public RegisterRequestValidator(
			IMessageService msg,
			IValidationService validationService)
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage(msg.Validation("EmailRequired"))
				.EmailAddress().WithMessage(msg.Validation("EmailInvalid"))
				.MustAsync(async (email, cancellation) =>
					await validationService.IsEmailUniqueAsync(email, cancellation))
				.WithMessage(msg.Business("EmailExists"));

			RuleFor(x => x.Password)
				.NotEmpty().WithMessage(msg.Validation("PasswordRequired"))
				.MinimumLength(6).WithMessage(msg.Validation("PasswordMinLength"));

			RuleFor(x => x.FullName)
				.NotEmpty().WithMessage(msg.Validation("FullNameRequired"));
		}
	}
}