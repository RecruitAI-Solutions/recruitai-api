using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Repositories;
using RecruitAI.Infrastructure.Data;

namespace RecruitAI.Infrastructure.Repositories;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
	public NotificationRepository(RecruitDevContext context) : base(context)
	{
	}

	public async Task<(List<Notification> Items, int Total)> GetByUserIdAsync(
		Guid userId, int page, int pageSize, bool? isRead = null, CancellationToken cancellationToken = default)
	{
		var query = _dbSet
			.Where(n => n.UserId == userId)
			.OrderByDescending(n => n.CreatedAt);

		if (isRead.HasValue)
		{
			query = (IOrderedQueryable<Notification>)query.Where(n => n.IsRead == isRead.Value);
		}

		var total = await query.CountAsync(cancellationToken);

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return (items, total);
	}

	public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(n => n.UserId == userId && !n.IsRead)
			.CountAsync(cancellationToken);
	}

	public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
	{
		var notification = await _dbSet
			.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

		if (notification != null && !notification.IsRead)
		{
			notification.IsRead = true;
			Update(notification);
		}
	}

	public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		await _dbSet
			.Where(n => n.UserId == userId && !n.IsRead)
			.ExecuteUpdateAsync(setter => setter.SetProperty(n => n.IsRead, true), cancellationToken);
	}
}