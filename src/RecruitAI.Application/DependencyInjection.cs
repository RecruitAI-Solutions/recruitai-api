using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Services;
using RecruitAI.Domain.Interfaces.Services;
using RecruitAI.Domain.Services;
using RecruitAI.Infrastructure.Services;
using System.Reflection;
using RecruitAI.Application.Behaviors;
using RecruitAI.Application.Validators.Auths;
using RecruitAI.Application.Validators.CVs;
using RecruitAI.Application.Validators.AI;
using RecruitAI.Application.Validators.Emails;
using RecruitAI.Application.Validators.Jobs;
using RecruitAI.Infrastructure.BackgroundServices;
using RecruitAI.Infrastructure.Services.AI;

namespace RecruitAI.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			services.AddMediatR(cfg =>
			{
				cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
				cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
				cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
			});

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
			services.AddScoped<IMatchingService, MatchingService>();
			services.AddScoped<IAuditLogService, AuditLogService>();
			services.AddScoped<IImageService, ImageService>();
			services.AddScoped<IAvatarCleanupService, AvatarCleanupService>();
			services.AddHostedService<AvatarCleanupBackgroundService>();
			services.AddScoped<IAIExtractionService, DeepSeekExtractionService>();
			services.AddScoped<IAIMatchingService, DeepSeekMatchingService>();
			services.AddScoped<ISkillService, SkillService>();

			//Validators
			services.AddScoped<RegisterRequestValidator>();
			services.AddScoped<LoginRequestValidator>();
			services.AddScoped<ForgotPasswordRequestValidator>();
			services.AddScoped<ResetPasswordRequestValidator>();
			services.AddScoped<UploadCVCommandValidator>();
			services.AddScoped<AnalyzeCvRequestValidator>();
			services.AddScoped<ChangePasswordRequestValidator>();
			services.AddScoped<UpdateProfileCommandValidator>();
			services.AddScoped<VerifyEmailRequestValidator>();
			services.AddScoped<ApplyJobCommandValidator>();
			services.AddScoped<UpdateApplicationStatusValidator>();

			services.AddLocalization();

			return services;
		}
	}
}