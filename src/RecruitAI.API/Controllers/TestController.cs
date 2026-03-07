using Microsoft.AspNetCore.Mvc;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Interfaces.Services;

namespace RecruitAI_API.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;
        public TestController(ITestService testService)
        {
            _testService = testService;
        }

        /// <summary>
        /// Lấy danh sách tất cả các item
        /// </summary>
        /// <returns>Danh sách TestItem</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Test>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var data = await _testService.GetAllTestsAsync();
            return Ok(data);
        }

        /// <summary>
        /// Tạo Test
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Test), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] Test test)
        {
            var createdTest = await _testService.AddTestAsync(test);
            return Ok(createdTest);
        }
    }
}
