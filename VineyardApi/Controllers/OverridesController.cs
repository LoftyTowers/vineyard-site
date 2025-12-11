using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Infrastructure;
using VineyardApi.Models;
using VineyardApi.Models.Requests;
using VineyardApi.Services;
using System.Collections.Generic;
using FluentValidation;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/overrides")]
    public class OverridesController : ControllerBase
    {
        private readonly IContentOverrideService _service;
        private readonly ILogger<OverridesController> _logger;

        private readonly IValidator<ContentOverride> _overrideValidator;
        private readonly IValidator<IdRequest> _idValidator;
        private readonly IValidator<RevertRequest> _revertValidator;

        public OverridesController(
            IContentOverrideService service, ILogger<OverridesController> logger,
            IValidator<ContentOverride> overrideValidator,
            IValidator<IdRequest> idValidator,
            IValidator<RevertRequest> revertValidator)
        {
            _service = service;
            _logger = logger;
            _overrideValidator = overrideValidator;
            _idValidator = idValidator;
            _revertValidator = revertValidator;
        }

        [HttpGet("{page}")]
        public async Task<IActionResult> GetOverrides(string page, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"Page", page}});
            _logger.LogInformation("Fetching overrides for page {Page}", page);
            var result = await _service.GetPublishedOverridesAsync(page, cancellationToken);
            return result.ToActionResult(this);
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("draft")]
        public async Task<IActionResult> SaveDraft([FromBody] ContentOverride model, CancellationToken cancellationToken)
        {
            var result = await _service.SaveDraftAsync(model, cancellationToken);
            return result.ToActionResult(this);
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("publish")]
        public async Task<IActionResult> PublishDraft([FromBody] IdRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.PublishDraftAsync(request.Id, cancellationToken);
            return result.ToActionResult(this);
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpGet("history/{page}/{blockKey}")]
        public async Task<IActionResult> GetHistory(string page, string blockKey, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Page"] = page,
                ["BlockKey"] = blockKey
            });

            var history = await _service.GetHistoryAsync(page, blockKey, cancellationToken);
            return history.ToActionResult(this);
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("revert")]
        public async Task<IActionResult> Revert([FromBody] RevertRequest request, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"OverrideId", request.Id}});
            _logger.LogInformation("Reverting override {OverrideId}", request.Id);
            var validationResult = await _revertValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var result = await _service.RevertAsync(request.Id, request.ChangedById, cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
