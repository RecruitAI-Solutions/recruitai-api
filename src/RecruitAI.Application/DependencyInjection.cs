using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Services;
using RecruitAI.Application.Validators;
using RecruitAI.Domain.Interfaces.Services;
using RecruitAI.Domain.Services;
using RecruitAI.Infrastructure.Services;
using System.Reflection;

namespace RecruitAI.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			services.AddMediatR(cfg =>
			cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

			services.AddAutoMapper(typeof(DependencyInjection));

			services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

			// Register application services here
			services.AddScoped<ITestService, TestService>();
			services.AddScoped<ITestDomainService, TestDomainService>();
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IMessageService, MessageService>();
			services.AddScoped<IValidationService, ValidationService>();
			services.AddScoped<IRefreshTokenService, RefreshTokenService>();
			services.AddScoped<IEmailService, EmailService>();


			//Validators
			services.AddScoped<RegisterRequestValidator>();
			services.AddScoped<LoginRequestValidator>();
			services.AddScoped<ForgotPasswordRequestValidator>();
			services.AddScoped<ResetPasswordRequestValidator>();

			services.AddLocalization();

			return services;
		}
	}
}