using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.AI;
using RecruitAI.Application.Interfaces.Services;
using System.Text;
using System.Text.Json;

namespace RecruitAI.Infrastructure.Services.AI;

public class DeepSeekRecommendationService : IAIRecommendationService
{
	private readonly ILogger<DeepSeekRecommendationService> _logger;
	private readonly HttpClient _httpClient;
	private readonly string _apiKey;
	private readonly string _model;
	private readonly string _apiUrl;
	private readonly bool _enabled;

	public DeepSeekRecommendationService(
		IConfiguration configuration,
		ILogger<DeepSeekRecommendationService> logger)
	{
		_logger = logger;
		_enabled = configuration.GetValue<bool>("AI:EnableRecommendation", true);
		_apiKey = configuration["DeepSeek:ApiKey"] ?? throw new InvalidOperationException("DeepSeek:ApiKey is not configured");
		_model = configuration["DeepSeek:Model"] ?? "deepseek-chat";
		_apiUrl = "https://api.deepseek.com/v1/chat/completions";

		_httpClient = new HttpClient();
		_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
	}

	public async Task<AIRecommendationDto> GetRecommendationAsync(
		List<string> matchedSkills,
		List<string> missingSkills,
		string jobTitle,
		List<string> requiredSkills,
		int applicationCount,
		CancellationToken cancellationToken = default)
	{
		if (!_enabled)
		{
			_logger.LogInformation("AI recommendation is disabled");
			return new AIRecommendationDto();
		}

		try
		{
			var prompt = GetPrompt(matchedSkills, missingSkills, jobTitle, requiredSkills, applicationCount);

			var requestBody = new
			{
				model = _model,
				messages = new[]
				{
					new { role = "system", content = GetSystemPrompt() },
					new { role = "user", content = prompt }
				},
				temperature = 0.3,
				max_tokens = 1500,
				response_format = new { type = "json_object" }
			};

			var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
			var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("AI Recommendation API Error: {StatusCode}", response.StatusCode);
				return new AIRecommendationDto();
			}

			return await ParseResponseAsync(responseJson);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "AI Recommendation failed");
			return new AIRecommendationDto();
		}
	}

	private string GetSystemPrompt()
	{
		return @"Bạn là chuyên gia tư vấn nghề nghiệp. Nhiệm vụ: phân tích điểm mạnh, điểm yếu của ứng viên và đề xuất khóa học phù hợp.

			QUY TẮC:
			1. Chỉ trả về JSON, không giải thích thêm
			2. Điểm mạnh: dựa trên matched skills
			3. Điểm yếu: dựa trên missing skills
			4. Khóa học đề xuất: phải liên quan đến missing skills
			5. Cạnh tranh: dựa trên số lượng ứng viên (dưới 10: Thấp, 10-30: Trung bình, trên 30: Cao)
			6. Khả năng trúng tuyển: dựa trên tỷ lệ matched skills (trên 70%: Cao, 40-70%: Trung bình, dưới 40%: Thấp)

			VÍ DỤ OUTPUT:
			{
			  ""strengths"": [""Thành thạo C#"", ""Kinh nghiệm SQL""],
			  ""weaknesses"": [""Thiếu React"", ""Chưa có chứng chỉ""],
			  ""recommendations"": [""Học React qua dự án thực tế"", ""Tham gia khóa .NET Advanced""],
			  ""suggestedCourses"": [
				{""name"": ""React Complete Guide"", ""platform"": ""Udemy"", ""url"": ""https://udemy.com/react"", ""price"": 500000}
			  ],
			  ""estimatedCompetition"": ""Cao (45 ứng viên)"",
			  ""successProbability"": ""Trung bình""
			}";
	}

	private string GetPrompt(
		List<string> matchedSkills,
		List<string> missingSkills,
		string jobTitle,
		List<string> requiredSkills,
		int applicationCount)
	{
		return $@"
			Phân tích đơn ứng tuyển cho vị trí: {jobTitle}

			Kỹ năng hiện có: {string.Join(", ", matchedSkills)}
			Kỹ năng còn thiếu: {string.Join(", ", missingSkills)}
			Tất cả kỹ năng yêu cầu: {string.Join(", ", requiredSkills)}
			Số lượng ứng viên đã apply: {applicationCount}

			Trả về JSON phân tích chi tiết.";
	}

	private async Task<AIRecommendationDto> ParseResponseAsync(string responseJson)
	{
		using var doc = JsonDocument.Parse(responseJson);

		var resultText = doc.RootElement
			.GetProperty("choices")[0]
			.GetProperty("message")
			.GetProperty("content")
			.GetString();

		if (string.IsNullOrEmpty(resultText))
		{
			return new AIRecommendationDto();
		}

		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};

		return JsonSerializer.Deserialize<AIRecommendationDto>(resultText, options) ?? new AIRecommendationDto();
	}
}