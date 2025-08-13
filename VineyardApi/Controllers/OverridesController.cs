using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/overrides")]
    public class OverridesController : ControllerBase
    {
        private readonly IContentOverrideService _service;
        public OverridesController(IContentOverrideService service)
        {
            _service = service;
        }

        [HttpGet("{page}")]
        public async Task<IActionResult> GetOverrides(string page)
        {
            var result = await _service.GetPublishedOverridesAsync(page);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        public async Task<IActionResult> SaveDraft([FromBody] ContentOverride model)
        {
            await _service.SaveDraftAsync(model);
            return Ok();
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("publish")]
        public async Task<IActionResult> PublishDraft([FromBody] IdRequest request)
        {
            await _service.PublishDraftAsync(request.Id);
            return Ok();
        }

        [HttpGet("history/{page}/{blockKey}")]
        public async Task<IActionResult> GetHistory(string page, string blockKey)
        {
            var history = await _service.GetHistoryAsync(page, blockKey);
            return Ok(history);
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("revert")]
        public async Task<IActionResult> Revert([FromBody] RevertRequest request)
        {
            await _service.RevertAsync(request.Id, request.ChangedById);
            return Ok();
        }
    }

    public record IdRequest(Guid Id);
    public record RevertRequest(Guid Id, Guid ChangedById);
}
