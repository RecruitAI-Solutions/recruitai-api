// RecruitAI.Application/Interfaces/Services/IExportService.cs
using RecruitAI.Application.DTOs.Responses.Admin;

namespace RecruitAI.Application.Interfaces.Services;

public interface IExportService
{
	Task<byte[]> ExportUsersAsync(ExportFilterDto filter, CancellationToken cancellationToken = default);
	Task<byte[]> ExportJobsAsync(ExportFilterDto filter, CancellationToken cancellationToken = default);
	Task<byte[]> ExportApplicationsAsync(ExportFilterDto filter, CancellationToken cancellationToken = default);
	string GetContentType(string format);
	string GetFileExtension(string format);
}