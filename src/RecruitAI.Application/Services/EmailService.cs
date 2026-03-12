using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI.Infrastructure.Services
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmailService> _logger;

		public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
		{
			_configuration = configuration;
			_logger = logger;
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

		public async Task SendPasswordResetEmailAsync(string to, string resetLink, CancellationToken cancellationToken = default)
		{
			var subject = "Reset Your Password - RecruitAI";

			var htmlBody = $@"
			<!DOCTYPE html>
			<html>
			<head>
				<style>
					body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
					.container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
					.header {{ background-color: #4a90e2; color: white; padding: 20px; text-align: center; }}
					.content {{ padding: 20px; }}
					.button {{
						display: inline-block;
						padding: 10px 20px;
						background-color: #4a90e2;
						color: white;
						text-decoration: none;
						border-radius: 5px;
						margin-top: 20px;
					}}
					.footer {{ margin-top: 30px; font-size: 12px; color: #999; text-align: center; }}
				</style>
			</head>
			<body>
				<div class='container'>
					<div class='header'>
						<h2>RecruitAI - Reset Password</h2>
					</div>
					<div class='content'>
						<p>Hello,</p>
						<p>We received a request to reset your password. Click the button below to create a new password:</p>
						<p style='text-align: center;'>
							<a href='{resetLink}' class='button'>Reset Password</a>
						</p>
						<p>If the button doesn't work, copy and paste this link into your browser:</p>
						<p style='word-break: break-all;'><small>{resetLink}</small></p>
						<p>This link will expire in 24 hours.</p>
						<p>If you didn't request this, please ignore this email.</p>
					</div>
					<div class='footer'>
						<p>&copy; {DateTime.UtcNow.Year} RecruitAI. All rights reserved.</p>
					</div>
				</div>
			</body>
			</html>";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}

		public async Task SendVerificationEmailAsync(string to, string verificationLink, CancellationToken cancellationToken = default)
		{
			var subject = "Verify Your Email - RecruitAI";

			var htmlBody = $@"
			<!DOCTYPE html>
			<html>
			<head>
				<style>
					body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
					.container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
					.header {{ background-color: #4a90e2; color: white; padding: 20px; text-align: center; }}
					.content {{ padding: 20px; }}
					.button {{
						display: inline-block;
						padding: 10px 20px;
						background-color: #4a90e2;
						color: white;
						text-decoration: none;
						border-radius: 5px;
						margin-top: 20px;
					}}
					.footer {{ margin-top: 30px; font-size: 12px; color: #999; text-align: center; }}
				</style>
			</head>
			<body>
				<div class='container'>
					<div class='header'>
						<h2>RecruitAI - Verify Your Email</h2>
					</div>
					<div class='content'>
						<p>Welcome to RecruitAI!</p>
						<p>Please verify your email address by clicking the button below:</p>
						<p style='text-align: center;'>
							<a href='{verificationLink}' class='button'>Verify Email</a>
						</p>
						<p>If the button doesn't work, copy and paste this link:</p>
						<p style='word-break: break-all;'><small>{verificationLink}</small></p>
					</div>
					<div class='footer'>
						<p>&copy; {DateTime.UtcNow.Year} RecruitAI. All rights reserved.</p>
					</div>
				</div>
			</body>
			</html>";

			await SendHtmlEmailAsync(to, subject, htmlBody, cancellationToken);
		}
	}
}