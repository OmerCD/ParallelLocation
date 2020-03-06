using System;
using Cumulocity.SDK.MQTT;
using Microsoft.AspNetCore.Mvc;
using Parallel.Application.Services.CumulocityIOT;
using Parallel.Application.Services.CumulocityIOT.Models;

namespace Parallel.Main
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        private readonly CumulocityIOTService _cumulocityIotService;
        public TestController(CumulocityIOTService cumulocityIotService)
        {
            _cumulocityIotService = cumulocityIotService;
        }

        [HttpGet("device")]
        public IActionResult RegisterDevice()
        {
            try
            {
                IMqttClient _ = _cumulocityIotService.RegisterDevice(new CumulocityDevice
                {
                    ClientId = "193",
                    DeviceName = "TestProgramDevice"
                }).Result;
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex);
            }

            return Ok();
        }
        [HttpGet]
        public IActionResult Test()
        {
            return Ok();
        }
    }
}