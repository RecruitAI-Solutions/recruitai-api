using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.DTOs.Requests.Geocoding;
using RecruitAI.Application.DTOs.Responses.Geocoding;
using RecruitAI.Application.Interfaces;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Shared.DTOs;
using RecruitAI.Shared.Interfaces;

namespace RecruitAI_API.Controllers.v1;
/// <summary>
/// Tra cứu địa chỉ và bản đồ (sử dụng Vietmap API)
/// </summary>
[AllowAnonymous]
public class GeocodingController : BaseController
{
	private readonly IGeocodingService _geocodingService;

	public GeocodingController(
		IMediator mediator,
		ILogger<GeocodingController> logger,
		IMessageService messageService,
		IGeocodingService geocodingService,
		IWorkContext workContext)
		: base(mediator, logger, messageService, workContext)
	{
		_geocodingService = geocodingService;
	}

	/// <summary>
	/// Tìm kiếm địa chỉ theo text
	/// </summary>
	/// <param name="request">Thông tin tìm kiếm</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Danh sách địa chỉ gợi ý</returns>
	/// <response code="200">Thành công, trả về danh sách địa chỉ</response>
	/// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
	/// <response code="429">Quá nhiều request</response>
	/// <response code="500">Lỗi hệ thống</response>
	[HttpGet("search")]
	[ProducesResponseType(typeof(GeocodingResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<GeocodingResponseDto>> Search(
		[FromQuery] GeocodingRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<GeocodingResponseDto>(async () =>
		{
			if (!ModelState.IsValid)
			{
				// Sẽ được xử lý bởi ValidationException trong BaseController
				throw new FluentValidation.ValidationException("Invalid request");
			}

			var results = await _geocodingService.SearchAddressAsync(
				request.Text,
				request.Limit,
				request.Lat,
				request.Lng,
				request.CityId,
				request.WardId,
				request.DisplayType,
				cancellationToken);

			return new GeocodingResponseDto
			{
				Success = true,
				Message = $"Tìm thấy {results.Count} kết quả",
				Data = results,
				TraceId = HttpContext.TraceIdentifier
			};
		});
	}

	/// <summary>
	/// Lấy chi tiết địa chỉ theo ID
	/// </summary>
	/// <param name="refId">ID tham chiếu của địa chỉ</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Thông tin chi tiết địa chỉ</returns>
	[HttpGet("place/{refId}")]
	[ProducesResponseType(typeof(GeocodingResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<GeocodingResponseDto>> GetPlace(
		string refId,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<GeocodingResponseDto>(async () =>
		{
			var result = await _geocodingService.GetAddressDetailAsync(refId, cancellationToken);

			if (result == null)
			{
				return new GeocodingResponseDto
				{
					Success = false,
					Message = "Không tìm thấy địa chỉ",
					TraceId = HttpContext.TraceIdentifier
				};
			}

			return new GeocodingResponseDto
			{
				Success = true,
				Data = new List<AddressDto> { result },
				TraceId = HttpContext.TraceIdentifier
			};
		});
	}

	/// <summary>
	/// Reverse geocoding - từ tọa độ ra địa chỉ
	/// </summary>
	/// <param name="request">Tọa độ cần tra cứu</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Danh sách địa chỉ gần tọa độ</returns>
	[HttpGet("reverse")]
	[ProducesResponseType(typeof(GeocodingResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<GeocodingResponseDto>> Reverse(
		[FromQuery] ReverseGeocodingRequestDto request,
		CancellationToken cancellationToken)
	{
		return await ExecuteAsync<GeocodingResponseDto>(async () =>
		{
			if (!ModelState.IsValid)
			{
				throw new FluentValidation.ValidationException("Invalid request");
			}

			var results = await _geocodingService.ReverseGeocodingAsync(
				request.Lat,
				request.Lng,
				request.Radius,
				cancellationToken);

			return new GeocodingResponseDto
			{
				Success = true,
				Message = $"Tìm thấy {results.Count} kết quả",
				Data = results,
				TraceId = HttpContext.TraceIdentifier
			};
		});
	}
}