using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;

namespace VineyardApi.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("error/{code:int}")]
        public IActionResult HandleStatusCode(int code, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = HttpContext.TraceIdentifier,
                ["StatusCode"] = code
            });

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var message = code == 404 ? "Resource not found" : "An unexpected error occurred";
                Response.StatusCode = code;
                return Problem(title: message, statusCode: code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while handling status code {StatusCode}", code);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Error handling failed"));
            }
        }

        [Route("error")]
        public IActionResult HandleException(CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = HttpContext.TraceIdentifier
            });

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
                var exception = context?.Error;
                _logger.LogError(exception, "Unhandled exception");
                return Problem(title: "An unexpected error occurred", statusCode: 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while handling exception");
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Error handling failed"));
            }
        }
    }
}
