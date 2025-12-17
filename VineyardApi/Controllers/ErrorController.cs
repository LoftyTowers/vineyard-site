using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using System.Diagnostics;

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
                ["CorrelationId"] = ResolveCorrelationId(),
                ["StatusCode"] = code
            });

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var message = code == 404 ? "Resource not found" : "An unexpected error occurred";
                var problem = BuildProblemDetails(code, message);
                LogProblem(problem, null);
                return StatusCode(problem.Status ?? code, problem);
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
                ["CorrelationId"] = ResolveCorrelationId()
            });

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
                var exception = context?.Error;
                var problem = BuildProblemDetails(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
                LogProblem(problem, exception);
                return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while handling exception");
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Error handling failed"));
            }
        }

        private ProblemDetails BuildProblemDetails(int statusCode, string title)
        {
            var correlationId = ResolveCorrelationId();
            var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Instance = HttpContext.Request?.Path
            };

            problem.Extensions["traceId"] = traceId;
            problem.Extensions["correlationId"] = correlationId;
            return problem;
        }

        private string ResolveCorrelationId()
        {
            if (HttpContext.Items.TryGetValue("X-Correlation-ID", out var fromItems) && fromItems is string itemValue && !string.IsNullOrWhiteSpace(itemValue))
            {
                return itemValue;
            }

            if (HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue) && !string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue.ToString();
            }

            return HttpContext.TraceIdentifier;
        }

        private void LogProblem(ProblemDetails problem, Exception? exception)
        {
            problem.Extensions.TryGetValue("traceId", out var traceId);
            problem.Extensions.TryGetValue("correlationId", out var correlationId);

            _logger.LogError(exception,
                "Request failed with status {Status} at {Path} {Method} (TraceId: {TraceId}, CorrelationId: {CorrelationId})",
                problem.Status,
                HttpContext.Request?.Path.Value,
                HttpContext.Request?.Method,
                traceId,
                correlationId);
        }
    }
}
