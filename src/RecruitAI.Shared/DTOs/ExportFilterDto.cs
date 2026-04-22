// RecruitAI.Application/DTOs/Responses/Admin/ExportFilterDto.cs
namespace RecruitAI.Shared.DTOs;

public class ExportFilterDto
{
	public string? Format { get; set; } = "excel";
	public string? Role { get; set; }
	public int? Status { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public string? Keyword { get; set; }
	public int? MinMatch { get; set; }
}