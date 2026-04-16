using RecruitAI.Domain.Entities;

namespace RecruitAI.Domain.Interfaces.Repositories;

public interface INotificationRepository : IBaseRepository<Notification>
{
	Task<(List<Notification> Items, int Total)> GetByUserIdAsync(
		Guid userId, int page, int pageSize, bool? isRead = null, CancellationToken cancellationToken = default);

	Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

	Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);

	Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}