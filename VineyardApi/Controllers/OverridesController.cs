using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using VineyardApi.Models;
using VineyardApi.Models.Requests;
using VineyardApi.Services;

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
        public async Task<IActionResult> GetOverridesAsync(string page, CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Page"] = page
            });

            try
            {
                var result = await _service.GetPublishedOverridesAsync(page, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch overrides for page {Page}", page);
                return ResultMapper.ToActionResult(this, Result<Dictionary<string, string>>.Failure(ErrorCode.Unknown, "Failed to fetch overrides"));
            }
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("draft")]
        public async Task<IActionResult> SaveDraftAsync([FromBody] ContentOverride model, CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["PageId"] = model.PageId,
                ["BlockKey"] = model.BlockKey
            });

            try
            {
                var validation = await _overrideValidator.ValidateAsync(model, cancellationToken);
                if (!validation.IsValid)
                {
                    return ResultMapper.FromValidationResult(this, validation);
                }

                var result = await _service.SaveDraftAsync(model, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save draft override for page {PageId} block {BlockKey}", model.PageId, model.BlockKey);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Failed to save draft"));
            }
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("publish")]
        public async Task<IActionResult> PublishDraftAsync([FromBody] IdRequest request, CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Id"] = request.Id
            });

            try
            {
                var validation = await _idValidator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return ResultMapper.FromValidationResult(this, validation);
                }

                var result = await _service.PublishDraftAsync(request.Id, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish draft override {Id}", request.Id);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Failed to publish draft"));
            }
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpGet("history/{page}/{blockKey}")]
        public async Task<IActionResult> GetHistoryAsync(string page, string blockKey, CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Page"] = page,
                ["BlockKey"] = blockKey
            });

            try
            {
                var history = await _service.GetHistoryAsync(page, blockKey, cancellationToken);
                return ResultMapper.ToActionResult(this, history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load history for page {Page} block {BlockKey}", page, blockKey);
                return ResultMapper.ToActionResult(this, Result<List<ContentOverride>>.Failure(ErrorCode.Unknown, "Failed to load history"));
            }
        }

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost("revert")]
        public async Task<IActionResult> RevertAsync([FromBody] RevertRequest request, CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Id"] = request.Id,
                ["ChangedById"] = request.ChangedById
            });

            try
            {
                var validation = await _revertValidator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return ResultMapper.FromValidationResult(this, validation);
                }

                var result = await _service.RevertAsync(request.Id, request.ChangedById, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revert override {Id}", request.Id);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Failed to revert override"));
            }
        }
    }

}
