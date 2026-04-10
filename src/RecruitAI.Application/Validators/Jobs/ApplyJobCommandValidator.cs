using FluentValidation;
using RecruitAI.Application.Commands.Applications;

namespace RecruitAI.Application.Validators.Jobs;

public class ApplyJobCommandValidator : AbstractValidator<ApplyJobCommand>
{
	public ApplyJobCommandValidator()
	{
		RuleFor(x => x.JobId)
			.NotEmpty().WithMessage("Job ID is required");

		RuleFor(x => x.CvId)
			.NotEmpty().WithMessage("CV ID is required");

		RuleFor(x => x.UserId)
			.NotEmpty().WithMessage("User ID is required");
	}
}