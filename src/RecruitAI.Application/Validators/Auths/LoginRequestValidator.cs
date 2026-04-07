using FluentValidation;
using RecruitAI.Application.DTOs.Requests.Auths;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Application.Validators.Auths
{
	public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
	{
		public LoginRequestValidator(IMessageService msg)
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage(msg.Validation("EmailRequired"))
				.EmailAddress().WithMessage(msg.Validation("EmailInvalid"));

			RuleFor(x => x.Password)
				.NotEmpty().WithMessage(msg.Validation("PasswordRequired"));
		}
	}
}