using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.DTOs.Responses;  
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Exceptions;  
using RecruitAI.Domain.Enums;  

namespace RecruitAI_API.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly ILogger<AuthController> _logger;
		private readonly IMessageService _msg;

		public AuthController(
			IAuthService authService,
			ILogger<AuthController> logger,
			IMessageService messageService)
		{
			_authService = authService;
			_logger = logger;
			_msg = messageService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
		{
			try
			{
				var result = await _authService.Register(request);
				_logger.LogInformation(_msg.Log("RegistrationSuccess"), request.Email);
				return Ok(result);
			}
			catch (BusinessException ex)
			{
				var response = new ErrorResponseDto
				{
					StatusCode = ex.StatusCode,
					ErrorCode = ex.ErrorCode,
					Message = ex.Message,
					TraceId = HttpContext.TraceIdentifier,
					Timestamp = DateTime.UtcNow
				};
				return StatusCode(ex.StatusCode, response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, _msg.Log("RegistrationError"), request.Email);
				var response = new ErrorResponseDto
				{
					StatusCode = 500,
					ErrorCode = ErrorCode.InternalServerError,
					Message = _msg.Business("InternalServerError"),
					TraceId = HttpContext.TraceIdentifier,
					Timestamp = DateTime.UtcNow
				};
				return StatusCode(500, response);
			}
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
		{
			try
			{
				var result = await _authService.Login(request);
				_logger.LogInformation(_msg.Log("LoginSuccess"), request.Email);
				return Ok(result);
			}
			catch (BusinessException ex)
			{
				var response = new ErrorResponseDto
				{
					StatusCode = ex.StatusCode,
					ErrorCode = ex.ErrorCode,
					Message = ex.Message,
					TraceId = HttpContext.TraceIdentifier,
					Timestamp = DateTime.UtcNow
				};
				return StatusCode(ex.StatusCode, response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, _msg.Log("LoginError"), request.Email);
				var response = new ErrorResponseDto
				{
					StatusCode = 500,
					ErrorCode = ErrorCode.InternalServerError,
					Message = _msg.Business("InternalServerError"),
					TraceId = HttpContext.TraceIdentifier,
					Timestamp = DateTime.UtcNow
				};
				return StatusCode(500, response);
			}
		}
	}
}