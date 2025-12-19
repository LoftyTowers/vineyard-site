using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using VineyardApi.Models;
using VineyardApi.Services;

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
        public async Task<IActionResult> GetThemeAsync(CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });

            try
            {
                var result = await _service.GetThemeAsync(cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled while loading theme");
                return ResultMapper.ToActionResult(this, Result<Dictionary<string, string>>.Failure(ErrorCode.Cancelled, "Request cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load theme");
                return ResultMapper.ToActionResult(this, Result<Dictionary<string, string>>.Failure(ErrorCode.Unexpected, "Failed to load theme"));
            }
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverrideAsync([FromBody] ThemeOverride model, CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["ThemeDefaultId"] = model.ThemeDefaultId,
                ["UpdatedById"] = model.UpdatedById
            });

            try
            {
                var validation = await _validator.ValidateAsync(model, cancellationToken);
                if (!validation.IsValid)
                {
                    return ResultMapper.FromValidationResult(this, validation);
                }

                var result = await _service.SaveOverrideAsync(model, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request cancelled while saving theme override for default {ThemeDefaultId}", model.ThemeDefaultId);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Cancelled, "Request cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save theme override for default {ThemeDefaultId}", model.ThemeDefaultId);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unexpected, "Failed to save theme override"));
            }
        }
    }
}
