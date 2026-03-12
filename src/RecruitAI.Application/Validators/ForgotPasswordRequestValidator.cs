using FluentValidation;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Application.Validators
{
	public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
	{
		public ForgotPasswordRequestValidator(IMessageService msg, IValidationService validationService)
		{
			RuleFor(x => x.Email)
			.MustAsync(async (email, ct) => !await validationService.IsEmailUniqueAsync(email, ct))
			.WithMessage(msg.Business("UserNotFound"));

			//Có thể kiểm tra email có tồn tại không (tùy chọn)
			RuleFor(x => x.Email)
				 .MustAsync(async (email, ct) => !await validationService.IsEmailUniqueAsync(email, ct))
				 .WithMessage(msg.Business("UserNotFound"));
		}
	}
}