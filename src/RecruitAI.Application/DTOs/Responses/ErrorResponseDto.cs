// RecruitAI.Application/DTOs/Responses/ErrorResponseDto.cs
using RecruitAI.Domain.Enums;

namespace RecruitAI.Application.DTOs.Responses
{
    public class ErrorResponseDto
    {
        public int StatusCode { get; set; }
        public ErrorCode ErrorCode { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } // Cho validation
        public string TraceId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}