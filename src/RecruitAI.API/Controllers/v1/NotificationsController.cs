using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.Notifications;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Notifications;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.Notifications;
using RecruitAI_API.Controllers.v1;

namespace RecruitAI.API.Controllers.v1;

/// <summary>
/// Quản lý thông báo của người dùng
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationsController : BaseController
{
	public NotificationsController(
		IMediator mediator,
		ILogger<NotificationsController> logger,
		IMessageService messageService,
		IWorkContext workContext)
		: base(mediator, logger, messageService, workContext)
	{
	}

	/// <summary>
	/// Lấy danh sách thông báo của người dùng hiện tại
	/// </summary>
	/// <param name="page">Số trang</param>
	/// <param name="pageSize">Số lượng mỗi trang</param>
	/// <param name="isRead">Lọc theo trạng thái đã đọc (true: đã đọc, false: chưa đọc, null: tất cả)</param>
	/// <returns>Danh sách thông báo phân trang</returns>
	[HttpGet]
	[ProducesResponseType(typeof(PaginationResponseDto<NotificationResponseDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<PaginationResponseDto<NotificationResponseDto>>> GetNotifications(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 20,
		[FromQuery] bool? isRead = null)
	{
		return await ExecuteAsync<PaginationResponseDto<NotificationResponseDto>>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var query = new GetNotificationsQuery
			{
				UserId = userId.Value,
				Page = page,
				PageSize = pageSize,
				IsRead = isRead
			};
			return await _mediator.Send(query);
		});
	}

	/// <summary>
	/// Lấy số lượng thông báo chưa đọc
	/// </summary>
	/// <returns>Số lượng thông báo chưa đọc</returns>
	[HttpGet("unread-count")]
	[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<int>> GetUnreadCount()
	{
		return await ExecuteAsync<int>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var query = new GetUnreadCountQuery { UserId = userId.Value };
			return await _mediator.Send(query);
		});
	}

	/// <summary>
	/// Đánh dấu một thông báo đã đọc
	/// </summary>
	/// <param name="id">ID thông báo</param>
	/// <returns>Kết quả đánh dấu</returns>
	[HttpPatch("{id}/read")]
	[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> MarkAsRead(Guid id)
	{
		return await ExecuteAsync(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var command = new MarkAsReadCommand { NotificationId = id, UserId = userId.Value };
			await _mediator.Send(command);
		}, _msg.Get("MarkAsReadSuccess"));
	}

	/// <summary>
	/// Đánh dấu tất cả thông báo đã đọc
	/// </summary>
	/// <returns>Kết quả đánh dấu</returns>
	[HttpPatch("read-all")]
	[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> MarkAllAsRead()
	{
		return await ExecuteAsync(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var command = new MarkAllAsReadCommand { UserId = userId.Value };
			await _mediator.Send(command);
		}, _msg.Get("MarkAllAsReadSuccess"));
	}
}