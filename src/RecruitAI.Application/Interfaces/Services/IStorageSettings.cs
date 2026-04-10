namespace RecruitAI.Application.Interfaces.Services
{
	public interface IStorageSettings
	{
		string AvatarPath { get; }
		string DefaultAvatarUrl { get; }
		int MaxFileSizeMB { get; }
		string[] AllowedExtensions { get; }
		int CleanupDays { get; }
	}
}