using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Models.Requests;
using VineyardApi.Services;
using FluentValidation;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/overrides")]
    public class OverridesController : ControllerBase
    {
        private readonly IContentOverrideService _service;
        private readonly IValidator<ContentOverride> _overrideValidator;
        private readonly IValidator<IdRequest> _idValidator;
        private readonly IValidator<RevertRequest> _revertValidator;

        public OverridesController(
            IContentOverrideService service,
            IValidator<ContentOverride> overrideValidator,
            IValidator<IdRequest> idValidator,
            IValidator<RevertRequest> revertValidator)
        {
            _service = service;
            _overrideValidator = overrideValidator;
            _idValidator = idValidator;
            _revertValidator = revertValidator;
        }

        [HttpGet("{page}")]
        public async Task<IActionResult> GetOverrides(string page)
        {
            var result = await _service.GetPublishedOverridesAsync(page);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        public async Task<IActionResult> SaveDraft([FromBody] ContentOverride model, CancellationToken cancellationToken)
        {
            var validationResult = await _overrideValidator.ValidateAsync(model, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            await _service.SaveDraftAsync(model);
            return Ok();
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("publish")]
        public async Task<IActionResult> PublishDraft([FromBody] IdRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _idValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

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
        public async Task<IActionResult> Revert([FromBody] RevertRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _revertValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            await _service.RevertAsync(request.Id, request.ChangedById);
            return Ok();
        }
    }
}
