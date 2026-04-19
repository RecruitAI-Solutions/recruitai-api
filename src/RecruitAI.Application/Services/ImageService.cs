using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
		private readonly IConfiguration _configuration;
		private readonly ILogger<ImageService> _logger;
		private readonly IStorageSettings _storageSettings;

		public ImageService(
			IWebHostEnvironment environment,
			IConfiguration configuration,
			ILogger<ImageService> logger,
			IStorageSettings storageSettings)
		{
			_environment = environment;
			_configuration = configuration;
			_logger = logger;
			_storageSettings = storageSettings;
		}

		private string GetPhysicalRootPath()
		{
			var useSeparatePath = _configuration.GetValue<bool>("FileStorage:UseSeparateUploadPath", false);

			if (useSeparatePath)
			{
				// PRODUCTION: Dùng thư mục riêng
				var uploadRoot = _configuration["FileStorage:UploadRootPath"];
				if (string.IsNullOrEmpty(uploadRoot))
				{
					throw new BusinessException(
						ErrorCode.ConfigurationError,
						"Upload root path not configured for Production");
				}
				return uploadRoot;
			}
			else
			{
				// DEVELOPMENT: Dùng wwwroot
				if (string.IsNullOrEmpty(_environment.WebRootPath))
				{
					throw new BusinessException(
						ErrorCode.ConfigurationError,
						"Web root path not configured");
				}
				return _environment.WebRootPath;
			}
		}

		private string GetFullPath(string subDirectory)
		{
			var rootPath = GetPhysicalRootPath();
			return Path.Combine(rootPath, subDirectory);
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

			// Lấy đường dẫn vật lý (hỗ trợ Dev/Prod)
			var uploadsFolder = GetFullPath(subDirectory);

			if (!Directory.Exists(uploadsFolder))
			{
				Directory.CreateDirectory(uploadsFolder);
			}

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
				var fullPath = GetFullPath(filePath);
				if (File.Exists(fullPath))
				{
					File.Delete(fullPath);
					_logger.LogInformation("Deleted image: {FilePath}", fullPath);
				}

				// Xóa thumbnail nếu có
				var directory = Path.GetDirectoryName(fullPath);
				var fileName = Path.GetFileName(fullPath);
				var thumbPath = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(fileName)}_thumb{Path.GetExtension(fileName)}");

				if (File.Exists(thumbPath))
				{
					File.Delete(thumbPath);
					_logger.LogInformation("Deleted thumbnail: {ThumbPath}", thumbPath);
				}

				return true;
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
				var fullPath = GetFullPath(directoryPath);
				if (Directory.Exists(fullPath))
				{
					Directory.Delete(fullPath, true);
					_logger.LogInformation("Deleted directory: {DirectoryPath}", fullPath);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete directory at {DirectoryPath}", directoryPath);
				return false;
			}
		}
	}
}