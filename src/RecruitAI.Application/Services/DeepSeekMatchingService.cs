using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.DTOs.AI;
using RecruitAI.Application.Interfaces.Services;
using System.Text;
using System.Text.Json;

namespace RecruitAI.Infrastructure.Services.AI
{
	public class DeepSeekMatchingService : IAIMatchingService
	{
		private readonly ILogger<DeepSeekMatchingService> _logger;
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly string _model;
		private readonly string _apiUrl;

		public DeepSeekMatchingService(
			IConfiguration configuration,
			ILogger<DeepSeekMatchingService> logger)
		{
			_logger = logger;
			_apiKey = configuration["DeepSeek:ApiKey"] ?? throw new InvalidOperationException("DeepSeek:ApiKey is not configured");
			_model = configuration["DeepSeek:Model"] ?? "deepseek-chat";
			_apiUrl = "https://api.deepseek.com/v1/chat/completions";

			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
		}

		public async Task<AIMatchResponseDto> EvaluateMatchAsync(
			string cvText,
			string jobTitle,
			string jobDescription,
			string jobRequirements,
			decimal? salaryMin,
			decimal? salaryMax,
			string location,
			CancellationToken cancellationToken = default)
		{
			var requestId = Guid.NewGuid().ToString()[..8];
			_logger.LogInformation("[DeepSeekMatch:{RequestId}] Evaluating match for CV and Job: {JobTitle}", requestId, jobTitle);

			var prompt = GetMatchPrompt(cvText, jobTitle, jobDescription, jobRequirements, salaryMin, salaryMax, location);

			var requestBody = new
			{
				model = _model,
				messages = new[]
				{
					new { role = "system", content = GetSystemPrompt() },
					new { role = "user", content = prompt }
				},
				temperature = 0.1,
				max_tokens = 2048,
				response_format = new { type = "json_object" }
			};

			var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
			var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("[DeepSeekMatch:{RequestId}] API Error: {StatusCode}", requestId, response.StatusCode);
				throw new Exception($"DeepSeek API error: {response.StatusCode}");
			}

			var result = await ParseResponseAsync(responseJson);
			_logger.LogInformation("[DeepSeekMatch:{RequestId}] Match result: {Score}%", requestId, result.MatchPercentage);

			return result;
		}

		private string GetSystemPrompt()
		{
			return @"Bạn là chuyên gia tuyển dụng với 10 năm kinh nghiệm. Nhiệm vụ: đánh giá mức độ phù hợp giữa CV của ứng viên và yêu cầu công việc.

				QUY TẮC:
				1. Chỉ trả về JSON, không giải thích thêm
				2. Đánh giá dựa trên 4 yếu tố:
				   - Kỹ năng (50%): Kỹ năng trong CV có khớp với yêu cầu không?
				   - Kinh nghiệm (25%): Số năm kinh nghiệm có phù hợp không?
				   - Mức lương (15%): Mức lương mong muốn có trong khoảng không?
				   - Địa điểm (10%): Có phù hợp không?
				3. Mỗi yếu tố chấm điểm từ 0-100
				4. Tổng điểm = (skill*0.5 + experience*0.25 + salary*0.15 + location*0.1)

				VÍ DỤ OUTPUT:
				{
				  ""totalScore"": 75,
				  ""skillMatch"": 80,
				  ""experienceMatch"": 70,
				  ""salaryMatch"": 100,
				  ""locationMatch"": 0,
				  ""skillsMatch"": [""C#"", ""SQL""],
				  ""skillsMissing"": [""React""],
				  ""experienceMatchBool"": true,
				  ""salaryMatchBool"": true,
				  ""locationMatchBool"": false,
				  ""reason"": ""Kỹ năng tốt nhưng thiếu React và không phù hợp địa điểm""
				}";
		}

		private string GetMatchPrompt(
			string cvText,
			string jobTitle,
			string jobDescription,
			string jobRequirements,
			decimal? salaryMin,
			decimal? salaryMax,
			string location)
		{
			var cvLength = cvText.Length;
			var truncatedCv = cvLength > 8000 ? cvText[..8000] + "\n...[CV truncated]" : cvText;

			var salaryInfo = "";
			if (salaryMin.HasValue && salaryMax.HasValue)
			{
				salaryInfo = $"Mức lương: {salaryMin.Value:N0} - {salaryMax.Value:N0} VND";
			}
			else if (salaryMin.HasValue)
			{
				salaryInfo = $"Mức lương tối thiểu: {salaryMin.Value:N0} VND";
			}
			else if (salaryMax.HasValue)
			{
				salaryInfo = $"Mức lương tối đa: {salaryMax.Value:N0} VND";
			}

			return $@"
				Đánh giá mức độ phù hợp giữa CV và công việc sau:

				=== THÔNG TIN CÔNG VIỆC ===
				Tiêu đề: {jobTitle}
				Mô tả: {jobDescription}
				Yêu cầu: {jobRequirements}
				{salaryInfo}
				Địa điểm: {location}

				=== CV ỨNG VIÊN ===
				{truncatedCv}

				=== YÊU CẦU ĐẦU RA ===
				Trả về JSON đánh giá chi tiết với thang điểm 0-100 cho từng yếu tố và tổng điểm.";
		}

		private async Task<AIMatchResponseDto> ParseResponseAsync(string responseJson)
		{
			using var doc = JsonDocument.Parse(responseJson);

			var resultText = doc.RootElement
				.GetProperty("choices")[0]
				.GetProperty("message")
				.GetProperty("content")
				.GetString();

			if (string.IsNullOrEmpty(resultText))
			{
				return new AIMatchResponseDto();
			}

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			var score = JsonSerializer.Deserialize<MatchScoreDto>(resultText, options);

			return new AIMatchResponseDto
			{
				MatchPercentage = score?.TotalScore ?? 0,
				SkillMatch = score?.SkillMatch ?? 0,
				ExperienceMatch = score?.ExperienceMatch ?? 0,
				SalaryMatch = score?.SalaryMatch ?? 0,
				LocationMatch = score?.LocationMatch ?? 0,
				MatchedSkills = score?.SkillsMatch ?? new(),
				MissingSkills = score?.SkillsMissing ?? new(),
				AiReason = score?.Reason ?? string.Empty,
				UsedAI = true,
				UsedCache = false
			};
		}
	}
}