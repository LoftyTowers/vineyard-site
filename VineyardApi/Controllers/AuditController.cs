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
        public async Task<IActionResult> GetRecent(CancellationToken cancellationToken)
        {
            var logs = await _service.GetRecentAsync(cancellationToken: cancellationToken);
            if (logs.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok(logs.Value);
        }
    }
}
