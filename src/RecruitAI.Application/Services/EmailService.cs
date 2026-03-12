using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace RecruitAI.Infrastructure.Services
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmailService> _logger;
		private readonly IEmailTemplateService _templateService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public EmailService(
			IConfiguration configuration,
			ILogger<EmailService> logger,
			IEmailTemplateService templateService,
			IHttpContextAccessor httpContextAccessor)
		{
			_configuration = configuration;
			_logger = logger;
			_templateService = templateService;
			_httpContextAccessor = httpContextAccessor;
		}

		private string GetUserLanguage()
		{
			var acceptLanguage = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
			return acceptLanguage?.StartsWith("vi") == true ? "vi" : "en";
		}

		public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
		{
			try
			{
				var smtpHost = _configuration["Email:SmtpHost"];
				var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
				var smtpUser = _configuration["Email:SmtpUser"];
				var smtpPass = _configuration["Email:SmtpPass"];
				var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@recruitai.com";
				var fromName = _configuration["Email:FromName"] ?? "RecruitAI";

				using var client = new SmtpClient(smtpHost, smtpPort)
				{
					EnableSsl = true,
					Credentials = new NetworkCredential(smtpUser, smtpPass)
				};

				var mailMessage = new MailMessage
				{
					From = new MailAddress(fromEmail, fromName),
					Subject = subject,
					Body = body,
					IsBodyHtml = false
				};
				mailMessage.To.Add(to);

				await client.SendMailAsync(mailMessage, cancellationToken);

				_logger.LogInformation("Email sent to {To}", to);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send email to {To}", to);
				throw;
			}
		}

		public async Task SendHtmlEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
		{
			try
			{
				var smtpHost = _configuration["Email:SmtpHost"];
				var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
				var smtpUser = _configuration["Email:SmtpUser"];
				var smtpPass = _configuration["Email:SmtpPass"];
				var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@recruitai.com";
				var fromName = _configuration["Email:FromName"] ?? "RecruitAI";

				using var client = new SmtpClient(smtpHost, smtpPort)
				{
					EnableSsl = true,
					Credentials = new NetworkCredential(smtpUser, smtpPass)
				};

				var mailMessage = new MailMessage
				{
					From = new MailAddress(fromEmail, fromName),
					Subject = subject,
					Body = htmlBody,
					IsBodyHtml = true
				};
				mailMessage.To.Add(to);

				await client.SendMailAsync(mailMessage, cancellationToken);

				_logger.LogInformation("HTML email sent to {To}", to);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send HTML email to {To}", to);
				throw;
			}
		}

		public async Task SendPasswordResetEmailAsync(string to, string resetLink, string? userName = null, CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("reset-password", language, cancellationToken);

			var displayName = userName ?? to.Split('@')[0];

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = displayName,
				["ResetLink"] = resetLink,
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi" ? "Đặt lại mật khẩu - RecruitAI" : "Reset Your Password - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}

		public async Task SendVerificationEmailAsync(string to, string verificationLink, string? userName = null, CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("verify-email", language, cancellationToken);

			var displayName = userName ?? to.Split('@')[0];

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = displayName,
				["VerificationLink"] = verificationLink,
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi" ? "Xác thực email - RecruitAI" : "Verify Your Email - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}
	}
}