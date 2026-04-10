using Microsoft.AspNetCore.Http;

namespace RecruitAI.Application.Interfaces.Services
{
	public interface IImageService
	{
		Task<(string fileName, string thumbnailFileName)> SaveImageAsync(
			IFormFile file,
			string subDirectory,
			CancellationToken cancellationToken = default);

		Task<bool> DeleteImageAsync(
			string filePath,
			CancellationToken cancellationToken = default);

		Task<bool> DeleteDirectoryAsync(
			string directoryPath,
			CancellationToken cancellationToken = default);
	}
}