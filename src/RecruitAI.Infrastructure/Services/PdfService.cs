// RecruitAI.Infrastructure/Services/PdfService.cs
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Interfaces.Services;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

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
				try
				{
					using var pdf = PdfDocument.Open(pdfStream);
					var text = new StringBuilder();

					foreach (var page in pdf.GetPages())
					{
						//Dùng GetWords để lấy từng từ có spacing
						var words = page.GetWords();
						var line = new StringBuilder();

						foreach (var word in words)
						{
							// Thêm khoảng trắng giữa các từ
							if (line.Length > 0)
								line.Append(' ');
							line.Append(word.Text);
						}

						text.AppendLine(line.ToString());
					}

					return CleanText(text.ToString());
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error extracting text from PDF stream");
					throw;
				}
			});
		}

		public async Task<string> ExtractTextAsync(string filePath)
		{
			return await Task.Run(() =>
			{
				try
				{
					using var pdf = PdfDocument.Open(filePath);
					var text = new StringBuilder();

					foreach (var page in pdf.GetPages())
					{
						//Dùng GetWords để lấy từng từ có spacing
						var words = page.GetWords();
						var line = new StringBuilder();

						foreach (var word in words)
						{
							// Thêm khoảng trắng giữa các từ
							if (line.Length > 0)
								line.Append(' ');
							line.Append(word.Text);
						}

						text.AppendLine(line.ToString());
					}

					return CleanText(text.ToString());
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error extracting text from PDF file: {FilePath}", filePath);
					throw;
				}
			});
		}

		/// <summary>
		/// Làm sạch text sau khi extract
		/// </summary>
		private string CleanText(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return text;

			// 1. Thay thế nhiều khoảng trắng liên tiếp bằng 1 khoảng trắng
			text = Regex.Replace(text, @"\s+", " ");

			// 2. Thêm khoảng trắng sau dấu phẩy nếu thiếu
			text = Regex.Replace(text, @",(?=[^\s])", ", ");

			// 3. Thêm khoảng trắng sau dấu chấm nếu thiếu
			text = Regex.Replace(text, @"\.(?=[^\s])", ". ");

			//Sửa ". NET" thành ".NET"
			text = Regex.Replace(text, @"\.\s+NET", ".NET", RegexOptions.IgnoreCase);

			//Sửa "ASP. NET" thành "ASP.NET"
			text = Regex.Replace(text, @"ASP\.\s+NET", "ASP.NET", RegexOptions.IgnoreCase);

			//Sửa ". NET Core" thành ".NET Core"
			text = Regex.Replace(text, @"\.\s+NET\s+Core", ".NET Core", RegexOptions.IgnoreCase);

			// 4. Thêm khoảng trắng sau dấu hai chấm nếu thiếu
			text = Regex.Replace(text, @":(?=[^\s])", ": ");

			// 5. Thêm khoảng trắng giữa chữ thường và chữ hoa (ví dụ: "BackendDeveloper" -> "Backend Developer")
			text = Regex.Replace(text, @"([a-z])([A-Z])", "$1 $2");
			// Sửa "Type Script" thành "TypeScript"
			text = Regex.Replace(text, @"Type\s+Script", "TypeScript", RegexOptions.IgnoreCase);

			// Sửa "Java Script" thành "JavaScript"
			text = Regex.Replace(text, @"Java\s+Script", "JavaScript", RegexOptions.IgnoreCase);

			// Sửa "React JS" thành "ReactJS" hoặc "React"
			text = Regex.Replace(text, @"React\s+JS", "React", RegexOptions.IgnoreCase);

			// 6. Thêm khoảng trắng giữa chữ và số (ví dụ: "C#" -> "C #")
			// text = Regex.Replace(text, @"([a-zA-Z])(\d)", "$1 $2");

			// 7. Xóa khoảng trắng thừa ở đầu và cuối
			text = text.Trim();

			return text;
		}
	}
}