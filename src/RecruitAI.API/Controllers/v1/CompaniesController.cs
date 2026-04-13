using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.Commands.Companies;
using RecruitAI.Application.DTOs.Common;
using RecruitAI.Application.DTOs.Companies;
using RecruitAI.Application.DTOs.Jobs;
using RecruitAI.Application.DTOs.Responses.Companies;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Queries.Companies;
using RecruitAI_API.Controllers.v1;

namespace RecruitAI.API.Controllers.v1;

/// <summary>
/// Quản lý thông tin công ty tuyển dụng
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class CompaniesController : BaseController
{
	public CompaniesController(
		IMediator mediator,
		ILogger<CompaniesController> logger,
		IMessageService messageService,
		IWorkContext workContext)
		: base(mediator, logger, messageService, workContext)
	{
	}

	/// <summary>
	/// Lấy danh sách công ty với phân trang và bộ lọc
	/// </summary>
	/// <remarks>
	/// **Chức năng:**
	/// - Hiển thị danh sách các công ty đang hoạt động
	/// - Hỗ trợ tìm kiếm theo tên công ty
	/// - Hỗ trợ phân trang và sắp xếp
	/// 
	/// **Bộ lọc:**
	/// - `keyword`: Tìm kiếm theo tên công ty (tùy chọn)
	/// - `sortBy`: Sắp xếp theo trường (name, createdAt)
	/// - `sortOrder`: Thứ tự sắp xếp (asc, desc)
	/// - `page`: Số trang (bắt đầu từ 1)
	/// - `pageSize`: Số lượng bản ghi mỗi trang (tối đa 50)
	/// </remarks>
	/// <param name="filter">Bộ lọc tìm kiếm công ty</param>
	/// <returns>Danh sách công ty phân trang</returns>
	/// <response code="200">Thành công, trả về danh sách công ty</response>
	[HttpGet]
	[AllowAnonymous]
	[ProducesResponseType(typeof(PaginationResponseDto<CompanyResponseDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PaginationResponseDto<CompanyResponseDto>>> GetCompanies(
		[FromQuery] CompanyFilterDto filter)
	{
		return await ExecuteAsync<PaginationResponseDto<CompanyResponseDto>>(async () =>
		{
			var query = new GetCompaniesQuery { Filter = filter };
			return await _mediator.Send(query);
		});
	}

	/// <summary>
	/// Lấy chi tiết công ty theo ID
	/// </summary>
	/// <remarks>
	/// **Chức năng:**
	/// - Xem thông tin chi tiết của một công ty
	/// - Bao gồm số lượng công việc đang tuyển của công ty
	/// 
	/// **Thông tin trả về:**
	/// - Tên công ty, slug, logo
	/// - Địa chỉ, website
	/// - Ngày tạo, số lượng công việc
	/// </remarks>
	/// <param name="id">ID của công ty</param>
	/// <returns>Thông tin chi tiết công ty</returns>
	/// <response code="200">Thành công, trả về thông tin công ty</response>
	/// <response code="404">Không tìm thấy công ty</response>
	[HttpGet("{id}")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(CompanyResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<CompanyResponseDto>> GetCompanyDetail(Guid id)
	{
		return await ExecuteAsync<CompanyResponseDto>(async () =>
		{
			var query = new GetCompanyDetailQuery { Id = id };
			return await _mediator.Send(query);
		});
	}

	/// <summary>
	/// Gợi ý công ty theo từ khóa (dùng cho autocomplete)
	/// </summary>
	/// <remarks>
	/// **Chức năng:**
	/// - Tìm kiếm và gợi ý công ty theo từ khóa nhập vào
	/// - Dùng cho ô tìm kiếm/autocomplete khi tạo công việc
	/// 
	/// **Cách dùng:**
	/// - Nhập vài ký tự đầu tiên của tên công ty
	/// - Hệ thống sẽ trả về danh sách công ty gợi ý
	/// 
	/// **Ví dụ:** `?q=tech` sẽ trả về các công ty có chứa "tech" trong tên
	/// </remarks>
	/// <param name="q">Từ khóa tìm kiếm (tối thiểu 1 ký tự)</param>
	/// <param name="limit">Số lượng kết quả tối đa (mặc định: 10, tối đa: 50)</param>
	/// <returns>Danh sách công ty gợi ý (ID, tên, logo)</returns>
	/// <response code="200">Thành công, trả về danh sách gợi ý</response>
	[HttpGet("suggest")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(List<CompanySuggestDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<CompanySuggestDto>>> SuggestCompanies(
		[FromQuery] string q = "",
		[FromQuery] int limit = 10)
	{
		return await ExecuteAsync<List<CompanySuggestDto>>(async () =>
		{
			var query = new SuggestCompaniesQuery { Keyword = q, Limit = limit };
			return await _mediator.Send(query);
		});
	}

	/// <summary>
	/// Cập nhật thông tin công ty
	/// </summary>
	/// <remarks>
	/// **Chức năng:**
	/// - Cập nhật thông tin của công ty (tên, logo, địa chỉ, website)
	/// - Chỉ chủ sở hữu (người tạo công ty) hoặc Admin mới được cập nhật
	/// 
	/// **Lưu ý:**
	/// - Khi cập nhật tên công ty, slug sẽ được tự động tạo lại
	/// - Các trường không gửi lên sẽ giữ nguyên giá trị cũ
	/// 
	/// **Quyền truy cập:**
	/// - RECRUITER: chỉ được cập nhật công ty do mình tạo
	/// - ADMIN: được cập nhật tất cả công ty
	/// </remarks>
	/// <param name="id">ID của công ty cần cập nhật</param>
	/// <param name="request">Thông tin cập nhật (tên, logo, địa chỉ, website)</param>
	/// <returns>Thông tin công ty sau khi cập nhật</returns>
	/// <response code="200">Thành công, trả về thông tin công ty đã cập nhật</response>
	/// <response code="401">Chưa đăng nhập</response>
	/// <response code="403">Không có quyền cập nhật công ty này</response>
	/// <response code="404">Không tìm thấy công ty</response>
	[HttpPut("{id}")]
	[Authorize]
	[ProducesResponseType(typeof(CompanyResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<CompanyResponseDto>> UpdateCompany(
		Guid id,
		[FromBody] CompanyUpdateDto request)
	{
		return await ExecuteAsync<CompanyResponseDto>(async () =>
		{
			var userId = GetCurrentUserId();
			if (userId == null)
				throw new UnauthorizedAccessException();

			var command = new UpdateCompanyCommand
			{
				Id = id,
				Name = request.Name,
				Logo = request.Logo,
				Address = request.Address,
				Website = request.Website,
				UserId = userId.Value
			};
			return await _mediator.Send(command);
		});
	}

	/// <summary>
	/// Lấy danh sách công việc của công ty
	/// </summary>
	/// <remarks>
	/// **Chức năng:**
	/// - Xem tất cả công việc đang tuyển của một công ty
	/// - Hỗ trợ phân trang
	/// 
	/// **Thông tin trả về cho mỗi công việc:**
	/// - Tiêu đề, địa điểm, mức lương
	/// - Loại hình công việc, cấp độ kinh nghiệm
	/// - Danh sách kỹ năng yêu cầu
	/// - Tên nhà tuyển dụng
	/// </remarks>
	/// <param name="id">ID của công ty</param>
	/// <param name="page">Số trang (bắt đầu từ 1)</param>
	/// <param name="pageSize">Số lượng công việc mỗi trang (tối đa 50)</param>
	/// <returns>Danh sách công việc của công ty (phân trang)</returns>
	/// <response code="200">Thành công, trả về danh sách công việc</response>
	[HttpGet("{id}/jobs")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(PaginationResponseDto<JobListDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PaginationResponseDto<JobListDto>>> GetCompanyJobs(
		Guid id,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 10)
	{
		return await ExecuteAsync<PaginationResponseDto<JobListDto>>(async () =>
		{
			var query = new GetCompanyJobsQuery
			{
				CompanyId = id,
				Page = page,
				PageSize = pageSize
			};
			return await _mediator.Send(query);
		});
	}
}