using Microsoft.AspNetCore.Mvc;
using VineyardApi.Infrastructure;
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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _service.LoginAsync(request.Username, request.Password);
            if (token == null)
            {
                return Result<string>.Failure(ErrorCode.BadRequest, "Invalid username or password")
                    .ToActionResult(this);
            }

            return Result<object>.Success(new { token }).ToActionResult(this);
        }
    }

    public record LoginRequest(string Username, string Password);
}
