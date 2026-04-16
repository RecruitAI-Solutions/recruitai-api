using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace RecruitAI.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
	private static readonly Dictionary<Guid, string> _userConnections = new();
	private readonly ILogger<NotificationHub> _logger;

	public NotificationHub(ILogger<NotificationHub> logger)
	{
		_logger = logger;
	}

	public override async Task OnConnectedAsync()
	{
		var userId = GetUserId();
		if (userId.HasValue)
		{
			_userConnections[userId.Value] = Context.ConnectionId;
			_logger.LogInformation("User {UserId} connected to NotificationHub. ConnectionId: {ConnectionId}",
				userId, Context.ConnectionId);
		}

		await Clients.Caller.SendAsync("Connected", new { Message = "Connected to notification hub", Timestamp = DateTime.UtcNow });
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var userId = GetUserId();
		if (userId.HasValue)
		{
			_userConnections.Remove(userId.Value);
			_logger.LogInformation("🔌 User {UserId} disconnected from NotificationHub", userId);
		}
		await base.OnDisconnectedAsync(exception);
	}

	/// <summary>
	/// Gửi thông báo đến user cụ thể
	/// </summary>
	public async Task SendToUser(Guid userId, object notification)
	{
		if (_userConnections.TryGetValue(userId, out var connectionId))
		{
			await Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
			_logger.LogDebug("Sent notification to user {UserId}", userId);
		}
		else
		{
			_logger.LogWarning("User {UserId} is not connected", userId);
		}
	}

	/// <summary>
	/// Gửi thông báo đến tất cả user đang kết nối
	/// </summary>
	public async Task SendToAll(object notification)
	{
		await Clients.All.SendAsync("ReceiveNotification", notification);
		_logger.LogDebug("Broadcast notification to all connected users");
	}

	/// <summary>
	/// Ping để kiểm tra kết nối
	/// </summary>
	public async Task Ping()
	{
		await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
	}

	private Guid? GetUserId()
	{
		var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
					   ?? Context.User?.FindFirst("sub")?.Value;

		if (Guid.TryParse(userIdClaim, out var userId))
			return userId;

		return null;
	}
}