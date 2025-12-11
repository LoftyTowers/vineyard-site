using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Domain.Content;
using VineyardApi.Infrastructure;
using VineyardApi.Models;
using VineyardApi.Services;
using System.Collections.Generic;
using FluentValidation;

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

        [HttpGet("{route}")]
        public async Task<IActionResult> GetPage(string route, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = HttpContext.TraceIdentifier,
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
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] PageOverride model, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = HttpContext.TraceIdentifier,
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
    }
}
