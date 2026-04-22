using FluentValidation;
using RecruitAI.Application.DTOs.Requests.Auths;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

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