using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RecruitAI.Application.Interfaces.Services;
using System.Text.Json;
using RecruitAI.Shared.DTOs;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.API.Middleware
{
	public class ValidationFilter : IAsyncActionFilter
	{
		private readonly IMessageService _msg;
		private readonly IServiceProvider _serviceProvider;

		public ValidationFilter(IMessageService msg, IServiceProvider serviceProvider)
		{
			_msg = msg;
			_serviceProvider = serviceProvider;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			// 1. Manual validation với async support
			foreach (var argument in context.ActionArguments)
			{
				if (argument.Value == null) continue;

				var validatorType = typeof(IValidator<>).MakeGenericType(argument.Value.GetType());
				var validator = _serviceProvider.GetService(validatorType) as IValidator;

				if (validator != null)
				{
					var validationContext = new ValidationContext<object>(argument.Value);
					var validationResult = await validator.ValidateAsync(validationContext);

					if (!validationResult.IsValid)
					{
						foreach (var error in validationResult.Errors)
						{
							context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
						}
					}
				}
			}

			// 2. Kiểm tra ModelState
			if (!context.ModelState.IsValid)
			{
				var errors = context.ModelState
					.Where(e => e.Value?.Errors.Count > 0)
					.ToDictionary(
						kvp => kvp.Key,
						kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
					);

				var response = new
				{
					StatusCode = 400,
					Message = _msg.Validation("ValidationFailed"),
					Errors = errors,
					TraceId = context.HttpContext.TraceIdentifier,
					Timestamp = DateTime.UtcNow
				};

				context.Result = new BadRequestObjectResult(response);
				return;
			}

			// 3. Tiếp tục pipeline
			await next();
		}
	}
}