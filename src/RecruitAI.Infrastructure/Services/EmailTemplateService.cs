using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.Infrastructure.Services
{
	public class EmailTemplateService : IEmailTemplateService
	{
		private readonly IHostEnvironment _env;
		private readonly ILogger<EmailTemplateService> _logger;
		private readonly string _templatePath;

		public EmailTemplateService(IHostEnvironment env, ILogger<EmailTemplateService> logger)
		{
			_env = env;
			_logger = logger;
			_templatePath = Path.Combine(_env.ContentRootPath, "Templates", "Emails");
		}

		public async Task<string> LoadTemplateAsync(string templateName, string language, CancellationToken cancellationToken = default)
		{
			try
			{
				// Thử load file HTML trước
				var htmlPath = Path.Combine(_templatePath, language, $"{templateName}.html");
				if (File.Exists(htmlPath))
				{
					return await File.ReadAllTextAsync(htmlPath, Encoding.UTF8, cancellationToken);
				}

				// Fallback sang file TXT
				var txtPath = Path.Combine(_templatePath, language, $"{templateName}.txt");
				if (File.Exists(txtPath))
				{
					return await File.ReadAllTextAsync(txtPath, Encoding.UTF8, cancellationToken);
				}

				// Fallback sang tiếng Anh
				var enPath = Path.Combine(_templatePath, "en", $"{templateName}.html");
				if (File.Exists(enPath))
				{
					_logger.LogWarning("Template {TemplateName} not found for language {Language}, falling back to English", templateName, language);
					return await File.ReadAllTextAsync(enPath, Encoding.UTF8, cancellationToken);
				}

				throw new FileNotFoundException($"Email template not found: {templateName}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error loading email template {TemplateName}", templateName);
				throw;
			}
		}

		public string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
		{
			var result = template;
			foreach (var placeholder in placeholders)
			{
				result = result.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
			}
			return result;
		}
	}
}