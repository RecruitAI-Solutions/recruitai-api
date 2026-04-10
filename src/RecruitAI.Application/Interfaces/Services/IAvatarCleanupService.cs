namespace RecruitAI.Application.Interfaces.Services
{
	public interface IAvatarCleanupService
	{
		Task CleanupOrphanedAvatarsAsync(bool force = false, CancellationToken cancellationToken = default);
	}
}