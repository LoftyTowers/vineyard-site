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
        public async Task<IActionResult> GetOverrides(string page, CancellationToken cancellationToken)
        {
            var result = await _service.GetPublishedOverridesAsync(page, cancellationToken);
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("draft")]
        public async Task<IActionResult> SaveDraft([FromBody] ContentOverride model, CancellationToken cancellationToken)
        {
            var result = await _service.SaveDraftAsync(model, cancellationToken);
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("publish")]
        public async Task<IActionResult> PublishDraft([FromBody] IdRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.PublishDraftAsync(request.Id, cancellationToken);
            if (result.IsFailure)
            {
                return result.Error == ErrorCode.NotFound ? NotFound() : StatusCode(500);
            }

            return Ok();
        }

        [Authorize]
        [HttpGet("history/{page}/{blockKey}")]
        public async Task<IActionResult> GetHistory(string page, string blockKey, CancellationToken cancellationToken)
        {
            var result = await _service.GetHistoryAsync(page, blockKey, cancellationToken);
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("revert")]
        public async Task<IActionResult> Revert([FromBody] RevertRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.RevertAsync(request.Id, request.ChangedById, cancellationToken);
            if (result.IsFailure)
            {
                return result.Error == ErrorCode.NotFound ? NotFound() : StatusCode(500);
            }

            return Ok();
        }
    }

    public record IdRequest(Guid Id);
    public record RevertRequest(Guid Id, Guid ChangedById);
}
