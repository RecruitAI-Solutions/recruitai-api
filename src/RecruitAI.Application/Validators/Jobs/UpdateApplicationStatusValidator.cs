using FluentValidation;
using RecruitAI.Application.Commands.Applications;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Validators.Jobs;

public class UpdateApplicationStatusValidator : AbstractValidator<UpdateApplicationStatusCommand>
{
	public UpdateApplicationStatusValidator()
	{
		RuleFor(x => x.ApplicationId)
			.NotEmpty().WithMessage("Application ID is required");

		RuleFor(x => x.RecruiterId)
			.NotEmpty().WithMessage("Recruiter ID is required");

		RuleFor(x => x.Status)
			.IsInEnum().WithMessage("Invalid status value");

		RuleFor(x => x.Notes)
			.MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
	}
}