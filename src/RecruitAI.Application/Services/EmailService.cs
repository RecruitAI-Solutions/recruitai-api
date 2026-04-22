using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using RecruitAI.Shared.Interfaces;

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

		public async Task SendJobApplicationEmailAsync(string to, string userName, string jobTitle,
			string companyName, string jobLocation, string salaryRange,
			DateTime appliedDate, string trackingLink, CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("job-application", language, cancellationToken);

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = userName,
				["JobTitle"] = jobTitle,
				["CompanyName"] = companyName,
				["JobLocation"] = jobLocation,
				["SalaryRange"] = salaryRange,
				["AppliedDate"] = appliedDate.ToString("dd/MM/yyyy HH:mm"),
				["TrackingLink"] = trackingLink,
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi"
				? $"Xác nhận ứng tuyển: {jobTitle} - RecruitAI"
				: $"Job Application Confirmation: {jobTitle} - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}

		public async Task SendInterviewInvitationEmailAsync(string to, string userName, string jobTitle,
			string companyName, DateTime interviewDate, string interviewTime,
			string interviewLocation, string interviewerName, int duration,
			string interviewFormat, string confirmLink, string declineLink,
			CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("interview-invitation", language, cancellationToken);

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = userName,
				["JobTitle"] = jobTitle,
				["CompanyName"] = companyName,
				["InterviewDate"] = interviewDate.ToString("dd/MM/yyyy"),
				["InterviewTime"] = interviewTime,
				["InterviewLocation"] = interviewLocation,
				["InterviewerName"] = interviewerName,
				["Duration"] = duration.ToString(),
				["InterviewFormat"] = interviewFormat,
				["ConfirmLink"] = confirmLink,
				["DeclineLink"] = declineLink,
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi"
				? $"Thư mời phỏng vấn: {jobTitle} - RecruitAI"
				: $"Interview Invitation: {jobTitle} - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}

		public async Task SendReminderEmailAsync(string to, string userName, string reminderSubject,
			string reminderMessage, DateTime reminderTime, CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("reminder", language, cancellationToken);

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = userName,
				["ReminderSubject"] = reminderSubject,
				["ReminderMessage"] = reminderMessage,
				["ReminderTime"] = reminderTime.ToString("dd/MM/yyyy HH:mm"),
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi"
				? $"Lời nhắc: {reminderSubject} - RecruitAI"
				: $"Reminder: {reminderSubject} - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}

		public async Task SendSystemAnnouncementEmailAsync(string to, string userName,
			string announcementTitle, string announcementContent, CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("system-announcement", language, cancellationToken);

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = userName,
				["AnnouncementTitle"] = announcementTitle,
				["AnnouncementContent"] = announcementContent,
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi"
				? $"Thông báo: {announcementTitle} - RecruitAI"
				: $"Announcement: {announcementTitle} - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}

		// ========== APPLICATION STATUS EMAILS ==========

		public async Task SendApplicationStatusUpdateEmailAsync(
			string to, string userName, string jobTitle, string companyName,
			string status, string? notes, string trackingLink,
			CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("application-status-update", language, cancellationToken);

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = userName,
				["JobTitle"] = jobTitle,
				["CompanyName"] = companyName,
				["Status"] = status,
				["Notes"] = notes ?? string.Empty,
				["TrackingLink"] = trackingLink,
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi"
				? $"Cập nhật trạng thái ứng tuyển: {jobTitle} - RecruitAI"
				: $"Application Status Update: {jobTitle} - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}

		public async Task SendApplicationReviewedEmailAsync(
			string to, string userName, string jobTitle, string companyName,
			string status, string? notes, string trackingLink,
			CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("application-reviewed", language, cancellationToken);

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = userName,
				["JobTitle"] = jobTitle,
				["CompanyName"] = companyName,
				["Status"] = status,
				["Notes"] = notes ?? string.Empty,
				["TrackingLink"] = trackingLink,
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi"
				? $"Hồ sơ của bạn đã được xem xét: {jobTitle} - RecruitAI"
				: $"Your application has been reviewed: {jobTitle} - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}

		public async Task SendApplicationAcceptedEmailAsync(
			string to, string userName, string jobTitle, string companyName,
			string status, string? nextSteps, string trackingLink,
			CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("application-accepted", language, cancellationToken);

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = userName,
				["JobTitle"] = jobTitle,
				["CompanyName"] = companyName,
				["Status"] = status,
				["NextSteps"] = nextSteps ?? string.Empty,
				["TrackingLink"] = trackingLink,
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi"
				? $"Chúc mừng! Bạn đã được chấp nhận: {jobTitle} - RecruitAI"
				: $"Congratulations! You have been accepted: {jobTitle} - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}

		public async Task SendApplicationRejectedEmailAsync(
			string to, string userName, string jobTitle, string companyName,
			string reason, string trackingLink,
			CancellationToken cancellationToken = default)
		{
			var language = GetUserLanguage();
			var template = await _templateService.LoadTemplateAsync("application-rejected", language, cancellationToken);

			var placeholders = new Dictionary<string, string>
			{
				["UserName"] = userName,
				["JobTitle"] = jobTitle,
				["CompanyName"] = companyName,
				["Reason"] = reason,
				["TrackingLink"] = trackingLink,
				["Year"] = DateTime.UtcNow.Year.ToString()
			};

			var htmlBody = _templateService.ReplacePlaceholders(template, placeholders);
			var subject = language == "vi"
				? $"Cập nhật đơn ứng tuyển: {jobTitle} - RecruitAI"
				: $"Application Update: {jobTitle} - RecruitAI";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}
	}
}