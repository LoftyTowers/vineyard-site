using System;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Ok(new { status = "Healthy", checkedAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Health check failed"));
            }
        }
    }
}
