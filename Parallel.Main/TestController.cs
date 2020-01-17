using Microsoft.AspNetCore.Mvc;

namespace Parallel.Main
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        public TestController()
        {
            
        }
        [HttpGet]
        public IActionResult Test()
        {
            return Ok();
        }
    }
}