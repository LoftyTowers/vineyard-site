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

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var tokenResult = await _service.LoginAsync(request.Username, request.Password, cancellationToken);
            if (tokenResult.IsFailure)
            {
                return tokenResult.Error == ErrorCode.Unauthorized ? Unauthorized() : StatusCode(500);
            }

            return Ok(new { token = tokenResult.Value });
        }
    }

    public record LoginRequest(string Username, string Password);
}
