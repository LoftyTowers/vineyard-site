using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models.Requests;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly IValidator<LoginRequest> _validator;

        public AuthController(IAuthService service, IValidator<LoginRequest> validator)
        {
            _service = service;
            _validator = validator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var token = await _service.LoginAsync(request.Username, request.Password);
            if (token == null) return Unauthorized();
            return Ok(new { token });
        }
    }
}
