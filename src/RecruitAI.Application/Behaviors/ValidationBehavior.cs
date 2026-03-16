using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	private readonly IEnumerable<IValidator<TRequest>> _validators;
	private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

	public ValidationBehavior(
		IEnumerable<IValidator<TRequest>> validators,
		ILogger<ValidationBehavior<TRequest, TResponse>> logger)
	{
		_validators = validators;
		_logger = logger;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!_validators.Any())
		{
			return await next();
		}

		var context = new ValidationContext<TRequest>(request);
		var validationResults = await Task.WhenAll(
			_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

		var failures = validationResults
			.SelectMany(r => r.Errors)
			.Where(f => f != null)
			.ToList();

		if (failures.Any())
		{
			_logger.LogWarning("Validation failed for {RequestType}: {Errors}",
				typeof(TRequest).Name,
				string.Join(", ", failures.Select(f => f.ErrorMessage)));

			// Ném ValidationException để Controller bắt và xử lý
			throw new ValidationException(failures);
		}

		return await next();
	}
}