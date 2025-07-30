using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult HandleStatusCode(int code)
        {
            var message = code == 404 ? "Resource not found" : "An unexpected error occurred";
            Response.StatusCode = code;
            return Problem(title: message, statusCode: code);
        }

        [Route("error")]
        public IActionResult HandleException()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;
            _logger.LogError(exception, "Unhandled exception");
            return Problem(title: "An unexpected error occurred", statusCode: 500);
        }
    }
}
