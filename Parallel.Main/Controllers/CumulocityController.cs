using Microsoft.AspNetCore.Mvc;
using Parallel.Application.Services.CumulocityIOT;
using Parallel.Application.Services.CumulocityIOT.Models;

namespace Parallel.Main.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CumulocityController : Controller
    {
        private CumulocityIOTService _cumulocityIotService;

        public CumulocityController(CumulocityIOTService cumulocityIotService)
        {
            _cumulocityIotService = cumulocityIotService;
        }

        [HttpGet]
        public IActionResult CumulocityTest()
        {
            // _cumulocityIotService.RegisterDevice(new CumulocityDevice()
            // {
            //     
            // })
            return Ok();
        }
    }
}