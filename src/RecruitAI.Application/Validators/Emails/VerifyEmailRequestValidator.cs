using FluentValidation;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Application.Validators.Emails
{
	public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequestDto>
	{
		public VerifyEmailRequestValidator(IMessageService msg)
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage(msg.Validation("EmailRequired"))
				.EmailAddress().WithMessage(msg.Validation("EmailInvalid"));

			RuleFor(x => x.Token)
				.NotEmpty().WithMessage(msg.Validation("TokenRequired"));
		}
	}
}