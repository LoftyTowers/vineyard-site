using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _service;
        public AuditController(IAuditService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent()
        {
            var logs = await _service.GetRecentAsync();
            return Ok(logs);
        }
    }
}
