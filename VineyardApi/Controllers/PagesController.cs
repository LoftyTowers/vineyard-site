using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;
using System.Collections.Generic;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/pages")]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _service;
        private readonly ILogger<PagesController> _logger;

        public PagesController(IPageService service, ILogger<PagesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{route}")]
        public async Task<IActionResult> GetPage(string route)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"PageRoute", route}});
            _logger.LogInformation("Fetching page content for {PageRoute}");

            var result = await _service.GetPageContentAsync(route);
            if (result == null)
            {
                _logger.LogWarning("No page content found for {PageRoute}", route);
                return NotFound();
            }

            _logger.LogInformation("Returning page content for {PageRoute}", route);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] PageOverride model)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"PageId", model.PageId}});
            _logger.LogInformation("Saving override for page {PageId}", model.PageId);
            await _service.SaveOverrideAsync(model);
            _logger.LogInformation("Override saved for page {PageId}", model.PageId);
            return Ok();
        }
    }
}
