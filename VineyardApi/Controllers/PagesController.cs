using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using VineyardApi.Domain.Content;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/pages")]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _service;
        private readonly ILogger<PagesController> _logger;
        private readonly IValidator<PageOverride> _validator;

        public PagesController(IPageService service, ILogger<PagesController> logger, IValidator<PageOverride> validator)
        {
            _service = service;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet("")]
        public Task<IActionResult> GetHomePageAsync(CancellationToken cancellationToken) =>
            GetPageAsync(string.Empty, cancellationToken);

        [HttpGet("{route}")]
        public async Task<IActionResult> GetPageAsync(string route, CancellationToken cancellationToken)
        {
            route = NormalizeRoute(route);
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Route"] = route
            });

            try
            {
                var result = await _service.GetPageContentAsync(route, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get page {Route}", route);
                return ResultMapper.ToActionResult(this, Result<PageContent>.Failure(ErrorCode.Unknown, "Failed to load page"));
            }
        }

        [Authorize]
        [HttpGet("{route}/draft")]
        public async Task<IActionResult> GetDraftAsync(string route, CancellationToken cancellationToken)
        {
            route = NormalizeRoute(route);
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Route"] = route
            });

            try
            {
                var result = await _service.GetDraftContentAsync(route, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get draft for page {Route}", route);
                return ResultMapper.ToActionResult(this, Result<PageContent>.Failure(ErrorCode.Unknown, "Failed to load draft"));
            }
        }

        [Authorize]
        [HttpPut("{route}/hero-image")]
        public async Task<IActionResult> UpdateHeroImageAsync(string route, [FromBody] UpdateHeroImageRequest model, CancellationToken cancellationToken)
        {
            route = NormalizeRoute(route);
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Route"] = route,
                ["ImageId"] = model.ImageId
            });

            try
            {
                if (model.ImageId == Guid.Empty)
                {
                    return ResultMapper.ToActionResult(this, Result<PageContent>.Failure(ErrorCode.Validation, "ImageId is required."));
                }

                var result = await _service.UpdateHeroImageAsync(route, model.ImageId, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update hero image for route {Route}", route);
                return ResultMapper.ToActionResult(this, Result<PageContent>.Failure(ErrorCode.Unknown, "Failed to update hero image"));
            }
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverrideAsync([FromBody] PageOverride model, CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["PageId"] = model.PageId,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save page override for page {PageId}", model.PageId);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Failed to save override"));
            }
        }

        [Authorize]
        [HttpPost("{route}/autosave")]
        public async Task<IActionResult> AutosaveAsync(string route, [FromBody] AutosaveDraftRequest model, CancellationToken cancellationToken)
        {
            route = NormalizeRoute(route);
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Route"] = route
            });

            try
            {
                if (model.Content == null)
                {
                    return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Validation, "Content is required."));
                }

                var result = await _service.AutosaveDraftAsync(route, model.Content, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to autosave draft for route {Route}", route);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Failed to autosave draft"));
            }
        }

        [Authorize]
        [HttpPost("{route}/publish")]
        public async Task<IActionResult> PublishDraftAsync(string route, CancellationToken cancellationToken)
        {
            route = NormalizeRoute(route);
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Route"] = route
            });

            try
            {
                var result = await _service.PublishDraftAsync(route, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish draft for route {Route}", route);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Failed to publish draft"));
            }
        }

        [Authorize]
        [HttpPost("{route}/discard")]
        public async Task<IActionResult> DiscardDraftAsync(string route, CancellationToken cancellationToken)
        {
            route = NormalizeRoute(route);
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Route"] = route
            });

            try
            {
                var result = await _service.DiscardDraftAsync(route, cancellationToken);
                return ResultMapper.ToActionResult(this, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to discard draft for route {Route}", route);
                return ResultMapper.ToActionResult(this, Result.Failure(ErrorCode.Unknown, "Failed to discard draft"));
            }
        }

        private static string NormalizeRoute(string route)
        {
            if (string.Equals(route, "home", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return route ?? string.Empty;
        }
    }
}
