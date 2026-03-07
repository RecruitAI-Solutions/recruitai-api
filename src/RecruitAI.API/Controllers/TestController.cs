using Microsoft.AspNetCore.Mvc;
using RecruitAI.Application.DTOs.Requests;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Domain.Entities;

namespace RecruitAI_API.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;
        private readonly ILogger<TestController> _logger;
        public TestController(ITestService testService, ILogger<TestController> logger)
        {
            _testService = testService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả các item
        /// </summary>
        /// <returns>Danh sách TestItem</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Test>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Received request to get all tests.");
                var data = await _testService.GetAllTestsAsync(cancellationToken);
                _logger.LogInformation("Successfully retrieved {Count} tests.", data.Count());
                return Ok(data);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request to get all tests was canceled.");
                return StatusCode(StatusCodes.Status499ClientClosedRequest, "The request was canceled by the client.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all tests.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Tạo Test
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Test), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CreatedTestRequestDto req, CancellationToken cancellationToken)
        {
            try
            {
                var createdTest = await _testService.AddTestAsync(req, cancellationToken);
                return Ok(createdTest);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request to create a test was canceled.");
                return StatusCode(StatusCodes.Status499ClientClosedRequest, "The request was canceled by the client.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a test.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
