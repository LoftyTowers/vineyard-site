using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Infrastructure;
using VineyardApi.Models;
using VineyardApi.Models.Requests;
using VineyardApi.Services;
using System.Collections.Generic;

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
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Username"] = request.Username
            });

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var token = await _service.LoginAsync(request.Username, request.Password);
            if (token == null)
            {
                _logger.LogWarning("Login failed for {Username}", request.Username);
                return Result<string>.Failure(ErrorCode.BadRequest, "Invalid username or password")
                    .ToActionResult(this);
            }

            _logger.LogInformation("Login succeeded for {Username}", request.Username);
            return Result<object>.Success(new { token }).ToActionResult(this);
        }
    }

    public record LoginRequest(string Username, string Password);
}
