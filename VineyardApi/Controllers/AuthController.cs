using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly IValidator<LoginRequest> _validator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService service, IValidator<LoginRequest> validator, ILogger<AuthController> logger)
        {
            _service = service;
            _validator = validator;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = HttpContext.TraceIdentifier,
                ["Username"] = request.Username
            });

            try
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return ResultMapper.FromValidationResult(this, validation);
                }

                var result = await _service.LoginAsync(request.Username, request.Password, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to login user {Username}", request.Username);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Login failed"));
            }
        }
    }

    public record LoginRequest(string Username, string Password);
}
