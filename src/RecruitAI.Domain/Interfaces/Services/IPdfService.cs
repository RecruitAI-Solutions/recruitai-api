// RecruitAI.Application/Interfaces/Services/IPdfService.cs
namespace RecruitAI.Domain.Interfaces.Services
{
	public interface IPdfService
	{
		Task<string> ExtractTextAsync(Stream pdfStream);
		Task<string> ExtractTextAsync(string filePath);
	}
}