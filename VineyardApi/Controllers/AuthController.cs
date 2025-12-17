using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using VineyardApi.Models;
using VineyardApi.Models.Requests;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly ILogger<AuthController> _logger;
        private readonly IValidator<LoginRequest> _validator;

        public AuthController(IAuthService service, ILogger<AuthController> logger, IValidator<LoginRequest> validator)
        {
            _service = service;
            _logger = logger;
            _validator = validator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
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
                if (!result.Success)
                {
                    return ResultMapper.ToActionResult(this, result);
                }

                return Ok(new { token = result.Value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to login user {Username}", request.Username);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Login failed"));
            }
        }
    }
}
