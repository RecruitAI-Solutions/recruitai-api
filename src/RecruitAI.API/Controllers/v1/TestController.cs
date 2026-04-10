//using Microsoft.AspNetCore.Mvc;
//using RecruitAI.Application.DTOs.Requests;
//using RecruitAI.Application.DTOs.Responses;
//using RecruitAI.Application.Interfaces.Services;

//namespace RecruitAI_API.Controllers.v1
//{
//	[Route("api/test")]
//	[ApiController]
//	public class TestController : ControllerBase
//	{
//		private readonly ITestService _testService;
//		private readonly ILogger<TestController> _logger;

//		public TestController(ITestService testService, ILogger<TestController> logger)
//		{
//			_testService = testService;
//			_logger = logger;
//		}

//		/// <summary>
//		/// Lấy danh sách tất cả các item
//		/// </summary>
//		/// <returns>Danh sách Test</returns>
//		[HttpGet]
//		[ProducesResponseType(typeof(IEnumerable<TestResponseDto>), StatusCodes.Status200OK)]
//		[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
//		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
//		public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
//		{
//			try
//			{
//				_logger.LogInformation("Received request to get all tests.");
//				var data = await _testService.GetAllTestsAsync(cancellationToken);
//				_logger.LogInformation("Successfully retrieved {Count} tests.", data.Count());
//				return Ok(data);
//			}
//			catch (OperationCanceledException)
//			{
//				_logger.LogWarning("Request to get all tests was canceled.");
//				return StatusCode(StatusCodes.Status499ClientClosedRequest, new
//				{
//					message = "The request was canceled by the client."
//				});
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "An error occurred while getting all tests.");
//				return StatusCode(StatusCodes.Status500InternalServerError, new
//				{
//					message = "An error occurred while processing your request."
//				});
//			}
//		}

//		/// <summary>
//		/// Tạo Test mới
//		/// </summary>
//		/// <param name="request">Thông tin test cần tạo</param>
//		/// <param name="cancellationToken">Cancellation token</param>
//		/// <returns>Test đã được tạo</returns>
//		[HttpPost]
//		[ProducesResponseType(typeof(TestResponseDto), StatusCodes.Status200OK)]
//		[ProducesResponseType(StatusCodes.Status400BadRequest)]
//		[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
//		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
//		public async Task<IActionResult> Create(
//			[FromBody] CreatedTestRequestDto request,
//			CancellationToken cancellationToken)
//		{
//			try
//			{
//				if (!ModelState.IsValid)
//				{
//					return BadRequest(ModelState);
//				}

//				_logger.LogInformation("Creating new test: {FirstName} {LastName}",
//					request.FirstName, request.LastName);

//				var createdTest = await _testService.AddTestAsync(request, cancellationToken);

//				_logger.LogInformation("Successfully created test with ID: {Id}", createdTest.Id);

//				return Ok(createdTest);
//			}
//			catch (OperationCanceledException)
//			{
//				_logger.LogWarning("Request to create a test was canceled.");
//				return StatusCode(StatusCodes.Status499ClientClosedRequest, new
//				{
//					message = "The request was canceled by the client."
//				});
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "An error occurred while creating a test.");
//				return StatusCode(StatusCodes.Status500InternalServerError, new
//				{
//					message = "An error occurred while processing your request."
//				});
//			}
//		}

//		/// <summary>
//		/// Lấy test theo ID
//		/// </summary>
//		[HttpGet("{id}")]
//		[ProducesResponseType(typeof(TestResponseDto), StatusCodes.Status200OK)]
//		[ProducesResponseType(StatusCodes.Status404NotFound)]
//		[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
//		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
//		public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
//		{
//			try
//			{
//				_logger.LogInformation("Getting test with ID: {Id}", id);
//				var test = await _testService.GetTestByIdAsync(id, cancellationToken);

//				if (test == null)
//				{
//					_logger.LogWarning("Test with ID {Id} not found", id);
//					return NotFound(new { message = $"Test with id {id} not found" });
//				}

//				return Ok(test);
//			}
//			catch (OperationCanceledException)
//			{
//				_logger.LogWarning("Request to get test by ID was canceled.");
//				return StatusCode(StatusCodes.Status499ClientClosedRequest, new
//				{
//					message = "The request was canceled by the client."
//				});
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Error getting test with ID {Id}", id);
//				return StatusCode(StatusCodes.Status500InternalServerError, new
//				{
//					message = "An error occurred while processing your request."
//				});
//			}
//		}

//		/// <summary>
//		/// Xóa test theo ID
//		/// </summary>
//		[HttpDelete("{id}")]
//		[ProducesResponseType(StatusCodes.Status200OK)]
//		[ProducesResponseType(StatusCodes.Status404NotFound)]
//		[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
//		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
//		public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
//		{
//			try
//			{
//				_logger.LogInformation("Deleting test with ID: {Id}", id);
//				var result = await _testService.DeleteTestAsync(id, cancellationToken);

//				if (!result)
//				{
//					_logger.LogWarning("Test with ID {Id} not found for deletion", id);
//					return NotFound(new { message = $"Test with id {id} not found" });
//				}

//				_logger.LogInformation("Successfully deleted test with ID: {Id}", id);
//				return Ok(new { message = "Test deleted successfully" });
//			}
//			catch (OperationCanceledException)
//			{
//				_logger.LogWarning("Request to delete test was canceled.");
//				return StatusCode(StatusCodes.Status499ClientClosedRequest, new
//				{
//					message = "The request was canceled by the client."
//				});
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Error deleting test with ID {Id}", id);
//				return StatusCode(StatusCodes.Status500InternalServerError, new
//				{
//					message = "An error occurred while processing your request."
//				});
//			}
//		}

//		/// <summary>
//		/// Tìm kiếm test theo tên
//		/// </summary>
//		[HttpGet("search")]
//		[ProducesResponseType(typeof(IEnumerable<TestResponseDto>), StatusCodes.Status200OK)]
//		[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
//		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
//		public async Task<IActionResult> Search([FromQuery] string name, CancellationToken cancellationToken)
//		{
//			try
//			{
//				_logger.LogInformation("Searching tests with name: {Name}", name);
//				var results = await _testService.SearchTestsByNameAsync(name, cancellationToken);
//				_logger.LogInformation("Found {Count} tests matching name: {Name}", results.Count(), name);
//				return Ok(results);
//			}
//			catch (OperationCanceledException)
//			{
//				_logger.LogWarning("Search request was canceled.");
//				return StatusCode(StatusCodes.Status499ClientClosedRequest, new
//				{
//					message = "The request was canceled by the client."
//				});
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Error searching tests by name {Name}", name);
//				return StatusCode(StatusCodes.Status500InternalServerError, new
//				{
//					message = "An error occurred while processing your request."
//				});
//			}
//		}
//	}
//}