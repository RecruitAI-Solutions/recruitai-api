// RecruitAI.Application/Validators/AI/AnalyzeCvRequestValidator.cs
using FluentValidation;
using RecruitAI.Application.DTOs.Requests.AI;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.DTOs;

namespace RecruitAI.Application.Validators.AI
{
	public class AnalyzeCvRequestValidator : AbstractValidator<AnalyzeCvRequestDto>
	{
		public AnalyzeCvRequestValidator(IMessageService msg)
		{
			RuleFor(x => x.CvId)
				.NotEmpty()
				.WithMessage(msg.Validation("CvIdRequired"));
		}
	}
}