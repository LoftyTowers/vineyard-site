using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;
using System.Collections.Generic;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/overrides")]
    public class OverridesController : ControllerBase
    {
        private readonly IContentOverrideService _service;
        private readonly ILogger<OverridesController> _logger;

        public OverridesController(IContentOverrideService service, ILogger<OverridesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{page}")]
        public async Task<IActionResult> GetOverrides(string page)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"Page", page}});
            _logger.LogInformation("Fetching overrides for page {Page}", page);
            var result = await _service.GetPublishedOverridesAsync(page);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        public async Task<IActionResult> SaveDraft([FromBody] ContentOverride model)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"Page", model.PageId}});
            _logger.LogInformation("Saving draft override for page {Page}", model.PageId);
            await _service.SaveDraftAsync(model);
            return Ok();
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("publish")]
        public async Task<IActionResult> PublishDraft([FromBody] IdRequest request)
        {
            _logger.LogInformation("Publishing draft override {OverrideId}", request.Id);
            await _service.PublishDraftAsync(request.Id);
            return Ok();
        }

        [HttpGet("history/{page}/{blockKey}")]
        public async Task<IActionResult> GetHistory(string page, string blockKey)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Page"] = page,
                ["BlockKey"] = blockKey
            });

            var history = await _service.GetHistoryAsync(page, blockKey);
            return Ok(history);
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("revert")]
        public async Task<IActionResult> Revert([FromBody] RevertRequest request)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"OverrideId", request.Id}});
            _logger.LogInformation("Reverting override {OverrideId}", request.Id);
            await _service.RevertAsync(request.Id, request.ChangedById);
            return Ok();
        }
    }

    public record IdRequest(Guid Id);
    public record RevertRequest(Guid Id, Guid ChangedById);
}
