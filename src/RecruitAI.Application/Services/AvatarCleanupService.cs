using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Shared.Interfaces;
using RecruitAI.Shared.Helpers;

namespace RecruitAI.Infrastructure.Services
{
	public class AvatarCleanupService : IAvatarCleanupService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _environment;
		private readonly ILogger<AvatarCleanupService> _logger;
		private readonly IStorageSettings _storageSettings;
		private readonly IConfiguration _configuration;

		public AvatarCleanupService(
			IUnitOfWork unitOfWork,
			IWebHostEnvironment environment,
			ILogger<AvatarCleanupService> logger,
			IStorageSettings storageSettings,
			IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_environment = environment;
			_logger = logger;
			_storageSettings = storageSettings;
			_configuration = configuration;
		}

		public async Task CleanupOrphanedAvatarsAsync(bool force = false, CancellationToken cancellationToken = default)
		{
			var avatarDirectory = GetAvatarDirectoryPath();
			if (string.IsNullOrEmpty(avatarDirectory) || !Directory.Exists(avatarDirectory))
				return;

			// Lấy danh sách avatar đang được dùng từ database qua UnitOfWork
			var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
			var userAvatarUrls = users
				.Where(u => !string.IsNullOrEmpty(u.AvatarUrl))
				.Select(u => u.AvatarUrl)
				.ToList();

			var userAvatarFileNames = userAvatarUrls
				.Select(url => Path.GetFileName(url))
				.ToHashSet();

			var allDirectories = Directory.GetDirectories(avatarDirectory);

			foreach (var dir in allDirectories)
			{
				var dirInfo = new DirectoryInfo(dir);
				var files = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);

				// Nhóm các file theo tên gốc (bỏ _thumb)
				var fileGroups = files.GroupBy(f =>
				{
					var name = Path.GetFileNameWithoutExtension(f.Name);
					if (name.EndsWith("_thumb"))
						name = name.Substring(0, name.Length - 6);
					return name;
				});

				foreach (var group in fileGroups)
				{
					var originalFileName = group.Key + group.First().Extension;
					var isReferenced = userAvatarFileNames.Contains(originalFileName);
					var isOld = (DateTime.UtcNow - group.Max(f => f.LastWriteTimeUtc)).TotalDays > _storageSettings.CleanupDays;
					var shouldDelete = force ? !isReferenced : (!isReferenced && isOld);

					if (shouldDelete)
					{
						foreach (var file in group)
						{
							try
							{
								file.Delete();
								_logger.LogInformation("Deleted orphaned avatar file: {FilePath}", file.FullName);
							}
							catch (Exception ex)
							{
								_logger.LogError(ex, "Failed to delete file: {FilePath}", file.FullName);
							}
						}
					}
				}

				// Xóa thư mục nếu rỗng
				if (!dirInfo.GetFiles().Any() && !dirInfo.GetDirectories().Any())
				{
					try
					{
						dirInfo.Delete();
						_logger.LogInformation("Deleted empty directory: {Directory}", dirInfo.FullName);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Failed to delete directory: {Directory}", dirInfo.FullName);
					}
				}
			}
		}

		private string GetAvatarDirectoryPath()
		{
			var useSeparatePath = _configuration.GetValue<bool>("FileStorage:UseSeparateUploadPath", false);

			if (useSeparatePath)
			{
				// PRODUCTION: Dùng thư mục riêng
				var uploadRoot = _configuration["FileStorage:UploadRootPath"];
				if (string.IsNullOrEmpty(uploadRoot))
				{
					_logger.LogWarning("Upload root path not configured for Production");
					return null;
				}
				return Path.Combine(uploadRoot, _storageSettings.AvatarPath);
			}
			else
			{
				// DEVELOPMENT: Dùng wwwroot
				if (string.IsNullOrEmpty(_environment.WebRootPath))
				{
					_logger.LogWarning("Web root path not configured");
					return null;
				}
				return Path.Combine(_environment.WebRootPath, _storageSettings.AvatarPath);
			}
		}
	}
}