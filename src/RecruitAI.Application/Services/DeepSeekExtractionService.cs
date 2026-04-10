using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.AI;
using RecruitAI.Application.Interfaces.Services;
using System.Text;
using System.Text.Json;

namespace RecruitAI.Infrastructure.Services.AI
{
	public class DeepSeekExtractionService : IAIExtractionService
	{
		private readonly ILogger<DeepSeekExtractionService> _logger;
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly string _model;
		private readonly string _apiUrl;
		private readonly string _systemPrompt;

		public DeepSeekExtractionService(
			IConfiguration configuration,
			ILogger<DeepSeekExtractionService> logger)
		{
			_logger = logger;
			_apiKey = configuration["DeepSeek:ApiKey"] ?? throw new InvalidOperationException("DeepSeek:ApiKey is not configured");
			_model = configuration["DeepSeek:Model"] ?? "deepseek-chat";
			_apiUrl = "https://api.deepseek.com/v1/chat/completions";

			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

			_systemPrompt = GetSystemPrompt();

			_logger.LogInformation("[DeepSeek] Initialized - Model: {Model}", _model);
		}

		public async Task<List<ExtractedSkillDto>> ExtractSkillsAsync(
			string cvText,
			CancellationToken cancellationToken = default)
		{
			var requestId = Guid.NewGuid().ToString()[..8];

			// Log request info
			_logger.LogInformation("[DeepSeek:{RequestId}] ========== START REQUEST ==========", requestId);
			_logger.LogInformation("[DeepSeek:{RequestId}] CV Length: {Length} chars", requestId, cvText?.Length ?? 0);
			_logger.LogDebug("[DeepSeek:{RequestId}] CV Preview: {Preview}", requestId, cvText?.Length > 500 ? cvText[..500] + "..." : cvText);

			var userMessage = GetUserMessage(cvText);

			// Log request body (JSON)
			var requestBody = new
			{
				model = _model,
				messages = new[]
				{
					new { role = "system", content = _systemPrompt },
					new { role = "user", content = userMessage }
				},
				temperature = 0.2,
				max_tokens = 2048,
				response_format = new { type = "json_object" }
			};

			var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = true });

			// Log request JSON (chỉ log ở level Debug hoặc Information)
			_logger.LogDebug("[DeepSeek:{RequestId}] Request JSON: {Json}", requestId, json);

			var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

			_logger.LogInformation("[DeepSeek:{RequestId}] Calling API...", requestId);

			var startTime = DateTime.UtcNow;
			var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
			var elapsed = DateTime.UtcNow - startTime;

			var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

			// Log response info
			_logger.LogInformation("[DeepSeek:{RequestId}] Response in {ElapsedMs}ms, Status: {StatusCode} ({(int)response.StatusCode})",
				requestId, elapsed.TotalMilliseconds, response.StatusCode);

			// Log response body (chỉ log khi có lỗi hoặc ở Debug level)
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("[DeepSeek:{RequestId}] Response Error Body: {ResponseBody}", requestId, responseJson);
			}
			else
			{
				_logger.LogDebug("[DeepSeek:{RequestId}] Response Body: {ResponseBody}", requestId, responseJson);
			}

			if (!response.IsSuccessStatusCode)
			{
				throw new Exception($"DeepSeek API error ({response.StatusCode}): {responseJson}");
			}

			// Parse response
			var skills = await ParseResponseAsync(responseJson);

			// Log extracted skills
			_logger.LogInformation("[DeepSeek:{RequestId}] Extracted {SkillCount} skills", requestId, skills.Count);
			foreach (var skill in skills)
			{
				_logger.LogDebug("[DeepSeek:{RequestId}] Skill: {Name} ({Category}) - Confidence: {Confidence}",
					requestId, skill.Name, skill.Category, skill.Confidence);
			}

			_logger.LogInformation("[DeepSeek:{RequestId}] ========== END REQUEST ==========", requestId);

			return skills;
		}

		private string GetSystemPrompt()
		{
			return @"Bạn là chuyên gia phân tích CV kỹ thuật. Nhiệm vụ: trích xuất kỹ năng chuyên môn từ CV.

			QUY TẮC:
			1. Chỉ trả về JSON hợp lệ, không giải thích thêm
			2. Mỗi kỹ năng gồm: name, category, confidence
			3. Category thuộc một trong: 'Programming Language', 'Framework', 'Database', 'Tool', 'Cloud', 'Other'
			4. Confidence từ 0.0 đến 1.0

			VÍ DỤ OUTPUT:
			{
			  ""skills"": [
				{""name"": ""C#"", ""category"": ""Programming Language"", ""confidence"": 0.95},
				{""name"": ""ASP.NET Core"", ""category"": ""Framework"", ""confidence"": 0.9}
			  ]
			}";
		}

		private string GetUserMessage(string cvText)
		{
			const int maxCvLength = 8000;
			var truncatedCv = cvText.Length > maxCvLength
				? cvText[..maxCvLength] + "\n...[CV truncated]"
				: cvText;

			return $@"Hãy phân tích CV sau và trả về danh sách kỹ năng:

CV:
{truncatedCv}";
		}

		private async Task<List<ExtractedSkillDto>> ParseResponseAsync(string responseJson)
		{
			using var doc = JsonDocument.Parse(responseJson);

			var resultText = doc.RootElement
				.GetProperty("choices")[0]
				.GetProperty("message")
				.GetProperty("content")
				.GetString();

			if (string.IsNullOrEmpty(resultText))
			{
				_logger.LogWarning("[DeepSeek] Empty response content");
				return new List<ExtractedSkillDto>();
			}

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			var response = JsonSerializer.Deserialize<SkillExtractionResponse>(resultText, options);
			return response?.Skills ?? new List<ExtractedSkillDto>();
		}

		private class SkillExtractionResponse
		{
			public List<ExtractedSkillDto> Skills { get; set; } = new();
		}
	}
}