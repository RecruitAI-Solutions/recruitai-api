using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.DTOs.Requests.Geocoding;
using RecruitAI.Application.DTOs.Responses.Geocoding;
using RecruitAI.Application.Interfaces.Services;

namespace RecruitAI_API.Controllers
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public class GeocodingController : ControllerBase
	{
		private readonly IGeocodingService _geocodingService;
		private readonly ILogger<GeocodingController> _logger;

		public GeocodingController(
			IGeocodingService geocodingService,
			ILogger<GeocodingController> logger)
		{
			_geocodingService = geocodingService;
			_logger = logger;
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
		[AllowAnonymous]
		[ProducesResponseType(typeof(GeocodingResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Search(
			[FromQuery] GeocodingRequestDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
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

				return Ok(new GeocodingResponseDto
				{
					Success = true,
					Message = $"Tìm thấy {results.Count} kết quả",
					Data = results,
					TraceId = HttpContext.TraceIdentifier
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error searching address: {Text}", request.Text);
				return StatusCode(500, new GeocodingResponseDto
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi tìm kiếm địa chỉ",
					TraceId = HttpContext.TraceIdentifier
				});
			}
		}

		/// <summary>
		/// Lấy chi tiết địa chỉ theo ID
		/// </summary>
		/// <param name="refId">ID tham chiếu của địa chỉ</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Thông tin chi tiết địa chỉ</returns>
		[HttpGet("place/{refId}")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(GeocodingResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetPlace(
			string refId,
			CancellationToken cancellationToken)
		{
			var result = await _geocodingService.GetAddressDetailAsync(refId, cancellationToken);

			if (result == null)
			{
				return NotFound(new GeocodingResponseDto
				{
					Success = false,
					Message = "Không tìm thấy địa chỉ",
					TraceId = HttpContext.TraceIdentifier
				});
			}

			return Ok(new GeocodingResponseDto
			{
				Success = true,
				Data = new List<AddressDto> { result },
				TraceId = HttpContext.TraceIdentifier
			});
		}

		/// <summary>
		/// Reverse geocoding - từ tọa độ ra địa chỉ
		/// </summary>
		/// <param name="request">Tọa độ cần tra cứu</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Danh sách địa chỉ gần tọa độ</returns>
		[HttpGet("reverse")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(GeocodingResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Reverse(
			[FromQuery] ReverseGeocodingRequestDto request,
			CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var results = await _geocodingService.ReverseGeocodingAsync(
				request.Lat,
				request.Lng,
				request.Radius,
				cancellationToken);

			return Ok(new GeocodingResponseDto
			{
				Success = true,
				Message = $"Tìm thấy {results.Count} kết quả",
				Data = results,
				TraceId = HttpContext.TraceIdentifier
			});
		}
	}
}
