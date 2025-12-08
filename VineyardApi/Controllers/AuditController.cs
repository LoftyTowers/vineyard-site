using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _service;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService service, ILogger<AuditController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent(CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = HttpContext.TraceIdentifier
            });

            try
            {
                var logs = await _service.GetRecentAsync(100, cancellationToken);
                return ResultMapper.ToActionResult(this, logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch audit logs");
                return ResultMapper.ToActionResult(this, Result<List<Models.AuditLog>>.Failure(ErrorCode.Unknown, "Failed to load logs"));
            }
        }
    }
}
