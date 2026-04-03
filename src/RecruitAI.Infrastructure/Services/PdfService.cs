// RecruitAI.Infrastructure/Services/PdfService.cs
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces.Services;
using System.Text;
using UglyToad.PdfPig;  
using UglyToad.PdfPig.Content;
using RecruitAI.Domain.Interfaces.Services;

namespace RecruitAI.Infrastructure.Services
{
	public class PdfService : IPdfService
	{
		private readonly ILogger<PdfService> _logger;

		public PdfService(ILogger<PdfService> logger)
		{
			_logger = logger;
		}

		public async Task<string> ExtractTextAsync(Stream pdfStream)
		{
			return await Task.Run(() =>
			{
				using var pdf = PdfDocument.Open(pdfStream);
				var text = new StringBuilder();

				foreach (var page in pdf.GetPages())
				{
					text.Append(page.Text);
				}

				return text.ToString();
			});
		}

		public async Task<string> ExtractTextAsync(string filePath)
		{
			return await Task.Run(() =>
			{
				using var pdf = PdfDocument.Open(filePath);
				var text = new StringBuilder();

				foreach (var page in pdf.GetPages())
				{
					text.Append(page.Text);
				}

				return text.ToString();
			});
		}
	}
}