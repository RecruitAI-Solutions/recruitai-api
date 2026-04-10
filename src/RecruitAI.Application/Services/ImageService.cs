using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace RecruitAI.Infrastructure.Services
{
	public class ImageService : IImageService
	{
		private readonly IWebHostEnvironment _environment;
		private readonly ILogger<ImageService> _logger;
		private readonly IStorageSettings _storageSettings;

		public ImageService(
			IWebHostEnvironment environment,
			ILogger<ImageService> logger,
			IStorageSettings storageSettings)
		{
			_environment = environment;
			_logger = logger;
			_storageSettings = storageSettings;
		}

		public async Task<(string fileName, string thumbnailFileName)> SaveImageAsync(
			IFormFile file,
			string subDirectory,
			CancellationToken cancellationToken = default)
		{
			// Kiểm tra kích thước
			if (file.Length > _storageSettings.MaxFileSizeMB * 1024 * 1024)
				throw new BusinessException(ErrorCode.FileTooLarge, $"File size exceeds {_storageSettings.MaxFileSizeMB}MB");

			var fileExtension = Path.GetExtension(file.FileName).ToLower();
			if (!_storageSettings.AllowedExtensions.Contains(fileExtension))
				throw new BusinessException(ErrorCode.InvalidFileType, "Invalid file format");

			var uploadsFolder = Path.Combine(_environment.WebRootPath, subDirectory);
			if (!Directory.Exists(uploadsFolder))
			{
				Directory.CreateDirectory(uploadsFolder);
			}

			// ✅ Xóa dòng khai báo fileExtension trùng này
			// var fileExtension = Path.GetExtension(file.FileName).ToLower();

			var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
			var thumbnailFileName = $"{Path.GetFileNameWithoutExtension(uniqueFileName)}_thumb{fileExtension}";

			var filePath = Path.Combine(uploadsFolder, uniqueFileName);
			var thumbnailPath = Path.Combine(uploadsFolder, thumbnailFileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream, cancellationToken);
			}

			using (var image = await Image.LoadAsync(filePath, cancellationToken))
			{
				var thumbnailSize = 150;
				var ratio = (double)thumbnailSize / Math.Max(image.Width, image.Height);
				var newWidth = (int)(image.Width * ratio);
				var newHeight = (int)(image.Height * ratio);

				image.Mutate(x => x.Resize(newWidth, newHeight));
				await image.SaveAsync(thumbnailPath, cancellationToken);
			}

			return (uniqueFileName, thumbnailFileName);
		}

		public async Task<bool> DeleteImageAsync(string filePath, CancellationToken cancellationToken = default)
		{
			try
			{
				var fullPath = Path.Combine(_environment.WebRootPath, filePath);
				if (File.Exists(fullPath))
				{
					File.Delete(fullPath);
				}
				return await Task.FromResult(true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete image at {FilePath}", filePath);
				return false;
			}
		}

		public async Task<bool> DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
		{
			try
			{
				var fullPath = Path.Combine(_environment.WebRootPath, directoryPath);
				if (Directory.Exists(fullPath))
				{
					Directory.Delete(fullPath, true);
				}
				return await Task.FromResult(true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete directory at {DirectoryPath}", directoryPath);
				return false;
			}
		}
	}
}