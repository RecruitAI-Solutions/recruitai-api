using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.Admin;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Requests.Admin;
using RecruitAI.Application.DTOs.Responses.Admin;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.Admin;
using RecruitAI.Domain.Enums;
using RecruitAI.Domain.Exceptions;
using RecruitAI.Infrastructure.Services;
using RecruitAI_API.Controllers.v1;

namespace RecruitAI.API.Controllers.v1
{
	/// <summary>
	/// Quản lý hệ thống - Chỉ dành cho Admin
	/// </summary>
	[ApiController]
	[Route("api/v1/admin")]
	[Authorize(Policy = "AdminOnly")]
	public class AdminController : BaseController
	{
		private readonly IAvatarCleanupService _avatarCleanupService;
		private readonly IExportService _exportService;

		public AdminController(
			IMediator mediator,
			ILogger<AdminController> logger,
			IMessageService messageService,
			IWorkContext workContext,
			IAvatarCleanupService avatarCleanupService,
			IExportService exportService)
			: base(mediator, logger, messageService, workContext)
		{
			_avatarCleanupService = avatarCleanupService;
			_exportService = exportService;
		}

		/// <summary>
		/// Lấy danh sách người dùng có phân trang và bộ lọc
		/// </summary>
		/// <param name="page">Số trang (bắt đầu từ 1)</param>
		/// <param name="pageSize">Số lượng bản ghi mỗi trang</param>
		/// <param name="role">Lọc theo vai trò (CANDIDATE, RECRUITER, ADMIN)</param>
		/// <param name="status">Lọc theo trạng thái (0: PendingVerification, 1: Active, 2: Suspended, 3: Banned)</param>
		/// <param name="keyword">Tìm kiếm theo email hoặc tên</param>
		/// <param name="sortBy">Sắp xếp theo trường (createdAt, email, fullName)</param>
		/// <param name="sortOrder">Thứ tự sắp xếp (asc, desc)</param>
		/// <returns>Danh sách người dùng</returns>
		[HttpGet("users")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserListResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<AdminUserListResponseDto>> GetUsers(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] string? role = null,
			[FromQuery] int? status = null,
			[FromQuery] string? keyword = null,
			[FromQuery] string sortBy = "createdAt",
			[FromQuery] string sortOrder = "desc")
		{
			return await ExecuteAsync<AdminUserListResponseDto>(async () =>
			{
				var query = new GetUsersListQuery
				{
					Page = page,
					PageSize = pageSize,
					Role = role,
					Status = status,
					Keyword = keyword,
					SortBy = sortBy,
					SortOrder = sortOrder
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Lấy chi tiết người dùng theo ID
		/// </summary>
		/// <param name="id">ID của người dùng</param>
		/// <returns>Thông tin chi tiết người dùng</returns>
		[HttpGet("users/{id}")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserDetailResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserDetailResponseDto>> GetUserDetail(Guid id)
		{
			return await ExecuteAsync<AdminUserDetailResponseDto>(async () =>
			{
				var query = new GetUserDetailQuery
				{
					UserId = id
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Cập nhật thông tin người dùng
		/// </summary>
		/// <param name="id">ID của người dùng</param>
		/// <param name="request">Thông tin cập nhật</param>
		/// <returns>Thông tin người dùng sau khi cập nhật</returns>
		[HttpPut("users/{id}")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserDetailResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserDetailResponseDto>> UpdateUser(
			Guid id,
			[FromBody] AdminUserUpdateRequestDto request)
		{
			return await ExecuteAsync<AdminUserDetailResponseDto>(async () =>
			{
				var command = new UpdateUserCommand
				{
					UserId = id,
					FullName = request.FullName,
					PhoneNumber = request.PhoneNumber,
					Gender = request.Gender,
					DateOfBirth = request.DateOfBirth,
					Role = request.Role,
					Status = request.Status
				};

				return await _mediator.Send(command);
			});
		}

		/// <summary>
		/// Cập nhật trạng thái người dùng (Active, Suspended, Banned)
		/// </summary>
		/// <param name="id">ID của người dùng</param>
		/// <param name="request">Trạng thái và lý do</param>
		/// <returns>Kết quả cập nhật</returns>
		[HttpPatch("users/{id}/status")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserStatusUpdateResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserStatusUpdateResponseDto>> UpdateUserStatus(
			Guid id,
			[FromBody] AdminUserStatusUpdateRequestDto request)
		{
			return await ExecuteAsync<AdminUserStatusUpdateResponseDto>(async () =>
			{
				var command = new UpdateUserStatusCommand
				{
					UserId = id,
					Status = request.Status,
					Reason = request.Reason
				};

				return await _mediator.Send(command);
			});
		}

		/// <summary>
		/// Cập nhật vai trò người dùng (CANDIDATE, RECRUITER, ADMIN)
		/// </summary>
		/// <param name="id">ID của người dùng</param>
		/// <param name="request">Vai trò mới</param>
		/// <returns>Kết quả cập nhật</returns>
		[HttpPatch("users/{id}/role")]
		[Authorize(Policy = "ManageRoles")]
		[ProducesResponseType(typeof(AdminUserRoleUpdateResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserRoleUpdateResponseDto>> UpdateUserRole(
			Guid id,
			[FromBody] AdminUserRoleUpdateRequestDto request)
		{
			return await ExecuteAsync<AdminUserRoleUpdateResponseDto>(async () =>
			{
				var command = new UpdateUserRoleCommand
				{
					UserId = id,
					Role = request.Role
				};

				return await _mediator.Send(command);
			});
		}

		/// <summary>
		/// Xóa người dùng (xóa mềm)
		/// </summary>
		/// <param name="id">ID của người dùng</param>
		/// <returns>Kết quả xóa</returns>
		[HttpDelete("users/{id}")]
		[Authorize(Policy = "ManageUsers")]
		[ProducesResponseType(typeof(AdminUserDeleteResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AdminUserDeleteResponseDto>> DeleteUser(Guid id)
		{
			return await ExecuteAsync<AdminUserDeleteResponseDto>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var command = new DeleteUserCommand
				{
					UserId = id,
					CurrentAdminId = userId.Value
				};

				return await _mediator.Send(command);
			});
		}
		/// <summary>
		/// Lấy thống kê tổng quan cho Dashboard Admin
		/// </summary>
		/// <param name="fromDate">Ngày bắt đầu (tùy chọn)</param>
		/// <param name="toDate">Ngày kết thúc (tùy chọn)</param>
		/// <returns>Thống kê số lượng người dùng, công việc, ứng tuyển</returns>
		[HttpGet("stats")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(StatsResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<StatsResponseDto>> GetStats(
		[FromQuery] DateTime? fromDate = null,
		[FromQuery] DateTime? toDate = null)
		{
			return await ExecuteAsync<StatsResponseDto>(async () =>
			{
				var query = new GetAdminStatsQuery
				{
					FromDate = fromDate,
					ToDate = toDate
				};
				return await _mediator.Send(query);
			});
		}
		/// <summary>
		/// Lấy danh sách nhật ký hệ thống với phân trang và bộ lọc
		/// </summary>
		/// <param name="page">Số trang</param>
		/// <param name="pageSize">Số lượng mỗi trang</param>
		/// <param name="entityType">Loại thực thể (User, CV, Job, Application)</param>
		/// <param name="action">Hành động (Create, Update, Delete, Login,...)</param>
		/// <param name="userId">Lọc theo ID người dùng</param>
		/// <param name="fromDate">Ngày bắt đầu</param>
		/// <param name="toDate">Ngày kết thúc</param>
		/// <param name="keyword">Từ khóa tìm kiếm</param>
		/// <param name="sortBy">Sắp xếp theo trường</param>
		/// <param name="sortOrder">Thứ tự sắp xếp</param>
		/// <returns>Danh sách nhật ký</returns>
		[HttpGet("audit-logs")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(PaginationResponseDto<AuditLogResponseDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<PaginationResponseDto<AuditLogResponseDto>>> GetAuditLogs(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 20,
			[FromQuery] string? entityType = null,
			[FromQuery] string? action = null,
			[FromQuery] Guid? userId = null,
			[FromQuery] DateTime? fromDate = null,
			[FromQuery] DateTime? toDate = null,
			[FromQuery] string? keyword = null,
			[FromQuery] string sortBy = "changedAt",
			[FromQuery] string sortOrder = "desc")
		{
			return await ExecuteAsync<PaginationResponseDto<AuditLogResponseDto>>(async () =>
			{
				var query = new GetAuditLogsQuery
				{
					Page = page,
					PageSize = pageSize,
					EntityType = entityType,
					Action = action,
					UserId = userId,
					FromDate = fromDate,
					ToDate = toDate,
					Keyword = keyword,
					SortBy = sortBy,
					SortOrder = sortOrder
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Lấy chi tiết nhật ký theo ID
		/// </summary>
		/// <param name="id">ID của nhật ký</param>
		/// <returns>Chi tiết nhật ký</returns>
		[HttpGet("audit-logs/{id}")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(AuditLogDetailResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AuditLogDetailResponseDto>> GetAuditLogDetail(Guid id)
		{
			return await ExecuteAsync<AuditLogDetailResponseDto>(async () =>
			{
				var query = new GetAuditLogDetailQuery { Id = id };
				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Lấy nhật ký của một thực thể cụ thể
		/// </summary>
		/// <param name="entityType">Loại thực thể</param>
		/// <param name="entityId">ID của thực thể</param>
		/// <param name="page">Số trang</param>
		/// <param name="pageSize">Số lượng mỗi trang</param>
		/// <returns>Danh sách nhật ký của thực thể</returns>
		[HttpGet("audit-logs/entity/{entityType}/{entityId}")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(PaginationResponseDto<AuditLogResponseDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<PaginationResponseDto<AuditLogResponseDto>>> GetEntityAuditLogs(
			string entityType,
			Guid entityId,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 20)
		{
			return await ExecuteAsync<PaginationResponseDto<AuditLogResponseDto>>(async () =>
			{
				if (!Enum.TryParse<AuditEntityType>(entityType, true, out var parsedEntityType))
				{
					throw new BusinessException(ErrorCode.InvalidData, "Invalid entity type");
				}

				var query = new GetEntityAuditLogsQuery
				{
					EntityType = parsedEntityType,
					EntityId = entityId.ToEntityId(),
					Page = page,
					PageSize = pageSize
				};

				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Dọn dẹp file avatar không còn liên kết (chỉ Admin)
		/// </summary>
		/// <param name="force">Dọn dẹp ngay cả khi có lỗi</param>
		/// <returns>Kết quả dọn dẹp</returns>
		[HttpPost("cleanup-avatars")]
		[Authorize(Policy = "AdminOnly")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> CleanupAvatars([FromQuery] bool force = false)
		{
			return await ExecuteAsync(async () =>
			{
				await _avatarCleanupService.CleanupOrphanedAvatarsAsync(force);
			}, "Avatar cleanup completed");
		}
		// ==================== EXPORT APIS ====================

		/// <summary>
		/// Xuất danh sách người dùng (Excel/CSV)
		/// </summary>
		/// <param name="format">Định dạng: excel hoặc csv (mặc định: excel)</param>
		/// <param name="role">Lọc theo vai trò (CANDIDATE, RECRUITER, ADMIN)</param>
		/// <param name="status">Lọc theo trạng thái</param>
		/// <param name="fromDate">Từ ngày</param>
		/// <param name="toDate">Đến ngày</param>
		/// <param name="keyword">Tìm kiếm theo email hoặc tên</param>
		[HttpGet("export/users")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> ExportUsers(
			[FromQuery] string? format = "excel",
			[FromQuery] string? role = null,
			[FromQuery] int? status = null,
			[FromQuery] DateTime? fromDate = null,
			[FromQuery] DateTime? toDate = null,
			[FromQuery] string? keyword = null)
		{
			var filter = new ExportFilterDto
			{
				Format = format,
				Role = role,
				Status = status,
				FromDate = fromDate,
				ToDate = toDate,
				Keyword = keyword
			};

			var data = await _exportService.ExportUsersAsync(filter);
			var contentType = _exportService.GetContentType(format);
			var extension = _exportService.GetFileExtension(format);
			var fileName = $"users_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";

			return File(data, contentType, fileName);
		}

		/// <summary>
		/// Xuất danh sách công việc (Excel/CSV)
		/// </summary>
		/// <param name="format">Định dạng: excel hoặc csv (mặc định: excel)</param>
		/// <param name="status">Lọc theo trạng thái</param>
		/// <param name="fromDate">Từ ngày</param>
		/// <param name="toDate">Đến ngày</param>
		/// <param name="keyword">Tìm kiếm theo tiêu đề</param>
		[HttpGet("export/jobs")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> ExportJobs(
			[FromQuery] string? format = "excel",
			[FromQuery] int? status = null,
			[FromQuery] DateTime? fromDate = null,
			[FromQuery] DateTime? toDate = null,
			[FromQuery] string? keyword = null)
		{
			var filter = new ExportFilterDto
			{
				Format = format,
				Status = status,
				FromDate = fromDate,
				ToDate = toDate,
				Keyword = keyword
			};

			var data = await _exportService.ExportJobsAsync(filter);
			var contentType = _exportService.GetContentType(format);
			var extension = _exportService.GetFileExtension(format);
			var fileName = $"jobs_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";

			return File(data, contentType, fileName);
		}

		/// <summary>
		/// Xuất danh sách đơn ứng tuyển (Excel/CSV)
		/// </summary>
		/// <param name="format">Định dạng: excel hoặc csv (mặc định: excel)</param>
		/// <param name="status">Lọc theo trạng thái</param>
		/// <param name="fromDate">Từ ngày</param>
		/// <param name="toDate">Đến ngày</param>
		/// <param name="minMatch">Lọc theo điểm match tối thiểu</param>
		[HttpGet("export/applications")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> ExportApplications(
			[FromQuery] string? format = "excel",
			[FromQuery] int? status = null,
			[FromQuery] DateTime? fromDate = null,
			[FromQuery] DateTime? toDate = null,
			[FromQuery] int? minMatch = null)
		{
			var filter = new ExportFilterDto
			{
				Format = format,
				Status = status,
				FromDate = fromDate,
				ToDate = toDate,
				MinMatch = minMatch
			};

			var data = await _exportService.ExportApplicationsAsync(filter);
			var contentType = _exportService.GetContentType(format);
			var extension = _exportService.GetFileExtension(format);
			var fileName = $"applications_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";

			return File(data, contentType, fileName);
		}

		// ==================== REPORT APIS ====================

		/// <summary>
		/// Báo cáo số lượng công việc theo tháng
		/// </summary>
		/// <param name="year">Năm (mặc định: năm hiện tại)</param>
		[HttpGet("reports/jobs-by-month")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(MonthlyReportDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<MonthlyReportDto>> GetJobsByMonthReport([FromQuery] int? year)
		{
			return await ExecuteAsync<MonthlyReportDto>(async () =>
			{
				var query = new GetJobsByMonthReportQuery { Year = year };
				return await _mediator.Send(query);
			});
		}

		/// <summary>
		/// Báo cáo số lượng đơn ứng tuyển theo tháng
		/// </summary>
		/// <param name="year">Năm (mặc định: năm hiện tại)</param>
		/// <param name="status">Lọc theo trạng thái</param>
		[HttpGet("reports/applications-by-month")]
		[Authorize(Policy = "ViewAnalytics")]
		[ProducesResponseType(typeof(MonthlyApplicationReportDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<MonthlyApplicationReportDto>> GetApplicationsByMonthReport(
			[FromQuery] int? year,
			[FromQuery] JobApplicationStatus? status = null)
		{
			return await ExecuteAsync<MonthlyApplicationReportDto>(async () =>
			{
				var query = new GetApplicationsByMonthReportQuery { Year = year, Status = status };
				return await _mediator.Send(query);
			});
		}
	}
}