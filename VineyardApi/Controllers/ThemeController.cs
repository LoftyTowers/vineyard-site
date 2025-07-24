using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("theme")]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _service;

        public ThemeController(IThemeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetTheme()
        {
            var result = await _service.GetThemeAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] ThemeOverride model)
        {
            await _service.SaveOverrideAsync(model);
            return Ok();
        }
    }
}
