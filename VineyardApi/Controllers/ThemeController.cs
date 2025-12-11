using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Infrastructure;
using VineyardApi.Models;
using VineyardApi.Services;
using System.Collections.Generic;
using FluentValidation;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/theme")]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _service;
        private readonly ILogger<ThemeController> _logger;
        private readonly IValidator<ThemeOverride> _validator;

        public ThemeController(IThemeService service, ILogger<ThemeController> logger, IValidator<ThemeOverride> validator)
        {
            _service = service;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> GetTheme(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching theme values");
            var result = await _service.GetThemeAsync(cancellationToken);
            return result.ToActionResult(this);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] ThemeOverride model, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"ThemeDefaultId", model.ThemeDefaultId}});
            _logger.LogInformation("Saving theme override {ThemeDefaultId}", model.ThemeDefaultId);
            var validationResult = await _validator.ValidateAsync(model, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var result = await _service.SaveOverrideAsync(model, cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
