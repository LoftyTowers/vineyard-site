using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/theme")]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _service;

        public ThemeController(IThemeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetTheme(CancellationToken cancellationToken)
        {
            var result = await _service.GetThemeAsync(cancellationToken);
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] ThemeOverride model, CancellationToken cancellationToken)
        {
            var result = await _service.SaveOverrideAsync(model, cancellationToken);
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok();
        }
    }
}
