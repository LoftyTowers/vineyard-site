using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;
using System.Collections.Generic;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/theme")]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _service;
        private readonly ILogger<ThemeController> _logger;

        public ThemeController(IThemeService service, ILogger<ThemeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTheme()
        {
            _logger.LogInformation("Fetching theme values");
            var result = await _service.GetThemeAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] ThemeOverride model)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"ThemeDefaultId", model.ThemeDefaultId}});
            _logger.LogInformation("Saving theme override {ThemeDefaultId}", model.ThemeDefaultId);
            await _service.SaveOverrideAsync(model);
            return Ok();
        }
    }
}
