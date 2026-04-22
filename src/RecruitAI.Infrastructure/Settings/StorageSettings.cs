using Microsoft.Extensions.Configuration;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI.Infrastructure.Settings
{
	public class StorageSettings : IStorageSettings
	{
		private readonly IConfiguration _configuration;

		public StorageSettings(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string AvatarPath => _configuration["Storage:AvatarPath"] ?? "uploads/avatars";
		public int MaxFileSizeMB => _configuration.GetValue<int>("Storage:MaxFileSizeMB", 5);
		public string[] AllowedExtensions => _configuration.GetSection("Storage:AllowedExtensions").Get<string[]>()
			?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
		public int CleanupDays => _configuration.GetValue<int>("Storage:CleanupDays", 30);
		public string DefaultAvatarUrl => _configuration["Storage:DefaultAvatarUrl"] ?? "/imgs/default_avatar/default.png";
	}
}