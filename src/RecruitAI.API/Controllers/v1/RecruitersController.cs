using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Responses.Recruiters;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.Jobs;
using RecruitAI.Application.Queries.Recruiters;
using RecruitAI.Domain.Enums;
using RecruitAI.Shared.Interfaces;
using RecruitAI_API.Controllers.v1;

namespace RecruitAI.API.Controllers.v1
{
	/// <summary>
	/// Quản lý dành cho nhà tuyển dụng
	/// </summary>
	[ApiController]
	[Route("api/v1/recruiters")]
	[Authorize(Roles = "RECRUITER")]
	public class RecruitersController : BaseController
	{
		public RecruitersController(
			IMediator mediator,
			ILogger<RecruitersController> logger,
			IMessageService messageService,
			IWorkContext workContext)
			: base(mediator, logger, messageService, workContext)
		{
		}

		/// <summary>
		/// Lấy danh sách tất cả ứng viên đã ứng tuyển vào các công việc của nhà tuyển dụng
		/// </summary>
		/// <param name="page">Số trang</param>
		/// <param name="pageSize">Số lượng mỗi trang</param>
		/// <param name="status">Lọc theo trạng thái đơn ứng tuyển</param>
		/// <param name="jobId">Lọc theo công việc cụ thể</param>
		/// <param name="minMatch">Lọc theo tỷ lệ phù hợp tối thiểu</param>
		/// <param name="query">Tìm theo tên ứng viên, email hoặc tên công việc</param>
		/// <param name="sortBy">Sắp xếp theo (appliedAt, matchPercentage, candidateName, jobtitle, status)</param>
		/// <param name="sortOrder">Thứ tự sắp xếp (asc/desc)</param>
		/// <returns>Danh sách ứng viên</returns>
		[HttpGet("candidates")]
		[ProducesResponseType(typeof(PaginationResponseDto<RecruiterCandidateDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<PaginationResponseDto<RecruiterCandidateDto>>> GetCandidates(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] JobApplicationStatus? status = null,
			[FromQuery] Guid? jobId = null,
			[FromQuery] int? minMatch = null,
			[FromQuery] string? query = null,
			[FromQuery] string? sortBy = "appliedAt",
			[FromQuery] string? sortOrder = "desc")
		{
			return await ExecuteAsync<PaginationResponseDto<RecruiterCandidateDto>>(async () =>
			{
				var userId = GetCurrentUserId();
				if (userId == null)
					throw new UnauthorizedAccessException();

				var candidatesQuery = new GetRecruiterCandidatesQuery
				{
					RecruiterId = userId.Value,
					Page = page,
					PageSize = pageSize,
					Status = status,
					JobId = jobId,
					MinMatch = minMatch,
					Query = query,
					SortBy = sortBy,
					SortOrder = sortOrder
				};

				return await _mediator.Send(candidatesQuery);
			});
		}
	}
}