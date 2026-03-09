using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.Interfaces.Services;

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

                // Log thành công (nếu cần)
                _logger.LogInformation(_msg.Log("RegistrationSuccess"), request.Email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Dùng resource cho log lỗi
                _logger.LogError(ex, _msg.Log("RegistrationError"), request.Email);

                // Dùng resource cho message trả về client
                return BadRequest(new
                {
                    message = _msg.Business("RegistrationFailed", ex.Message)
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.Login(request);

                // Log thành công
                _logger.LogInformation(_msg.Log("LoginSuccess"), request.Email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Dùng resource cho log lỗi
                _logger.LogError(ex, _msg.Log("LoginError"), request.Email);

                // Dùng resource cho message trả về client
                return Unauthorized(new
                {
                    message = _msg.Business("LoginFailed", ex.Message)
                });
            }
        }
    }
}