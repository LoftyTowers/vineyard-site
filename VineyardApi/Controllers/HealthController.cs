using System;
using Microsoft.AspNetCore.Mvc;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy", checkedAt = DateTime.UtcNow });
        }
    }
}
