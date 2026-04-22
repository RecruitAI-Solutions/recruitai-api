// RecruitAI.Shared/Interfaces/IExportService.cs
using RecruitAI.Shared.DTOs;

namespace RecruitAI.Shared.Interfaces;

public interface IExportService
{
	Task<byte[]> ExportUsersAsync(ExportFilterDto filter, CancellationToken cancellationToken = default);
	Task<byte[]> ExportJobsAsync(ExportFilterDto filter, CancellationToken cancellationToken = default);
	Task<byte[]> ExportApplicationsAsync(ExportFilterDto filter, CancellationToken cancellationToken = default);
	string GetContentType(string format);
	string GetFileExtension(string format);
}