using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
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

        public AuthController(IAuthService service, ILogger<AuthController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Username"] = request.Username
            });

            var token = await _service.LoginAsync(request.Username, request.Password);
            if (token == null)
            {
                _logger.LogWarning("Login failed for {Username}", request.Username);
                return Unauthorized();
            }

            _logger.LogInformation("Login succeeded for {Username}", request.Username);
            return Ok(new { token });
        }
    }

    public record LoginRequest(string Username, string Password);
}
