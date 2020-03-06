using Microsoft.AspNetCore.Mvc;
using Parallel.Application.ValueObjects;

namespace Parallel.Main.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : Controller
    {
        private readonly AppSettings _appSettings;

        public SettingsController(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        [HttpGet("signalr")]
        public IActionResult GetSignalRAddress()
        {
            return Ok(_appSettings.ApiConnectionInfo.IpAddress + ':' + _appSettings.ApiConnectionInfo.Port + '/' +
                      _appSettings.SignalRHub);
        }
    }
}