using FluentValidation;
using RecruitAI.Application.Commands.Auths;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Validators.Auths
{
	public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
	{
		public UpdateProfileCommandValidator()
		{
			RuleFor(x => x.FullName)
				.MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");

			RuleFor(x => x.PhoneNumber)
				.Matches(@"^(0|\+84)[3-9][0-9]{8}$")
				.When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
				.WithMessage("Invalid phone number format");

			RuleFor(x => x.Gender)
				.IsInEnum()
				.When(x => x.Gender.HasValue)
				.WithMessage("Gender must be Male, Female or Other");

			RuleFor(x => x.AvatarUrl)
				.MaximumLength(500).WithMessage("Avatar URL cannot exceed 500 characters");

			RuleFor(x => x.DateOfBirth)
				.LessThanOrEqualTo(DateTime.UtcNow)
				.When(x => x.DateOfBirth.HasValue)
				.WithMessage("Date of birth cannot be in the future");
		}
	}
}