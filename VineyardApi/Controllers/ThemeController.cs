using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;
using FluentValidation;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/theme")]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _service;
        private readonly IValidator<ThemeOverride> _validator;

        public ThemeController(IThemeService service, IValidator<ThemeOverride> validator)
        {
            _service = service;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> GetTheme()
        {
            var result = await _service.GetThemeAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] ThemeOverride model, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(model, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            await _service.SaveOverrideAsync(model);
            return Ok();
        }
    }
}
