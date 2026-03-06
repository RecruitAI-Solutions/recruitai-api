using Microsoft.AspNetCore.Mvc;
using RecruitAI.Domain.Entities;

namespace RecruitAI_API.Controllers
{
	[Route("api/test")]
	[ApiController]
	public class TestController : ControllerBase
	{
		/// <summary>
		/// Lấy danh sách tất cả các item
		/// </summary>
		/// <returns>Danh sách TestItem</returns>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<Test>), StatusCodes.Status200OK)]
		public IActionResult GetAll()
		{

			return Ok();
		}
	}
}
