namespace RecruitAI.Application.Interfaces.Services
{
	public interface IStorageSettings
	{
		string AvatarPath { get; }
		int MaxFileSizeMB { get; }
		string[] AllowedExtensions { get; }
		int CleanupDays { get; }
	}
}