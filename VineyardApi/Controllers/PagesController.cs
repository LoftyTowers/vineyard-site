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
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"PageRoute", route}});
            _logger.LogInformation($"Fetching page content for {route}");

            var result = await _service.GetPageContentAsync(route, cancellationToken);
            if (result.IsFailure)
            {
                _logger.LogWarning($"No page content found for {route}");
                return result.ToActionResult(this);
            }

            return result.ToActionResult(this);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] PageOverride model, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"PageId", model.PageId}});
            _logger.LogInformation($"Saving override for page {model.PageId}");
            var validationResult = await _validator.ValidateAsync(model, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var result = await _service.SaveOverrideAsync(model, cancellationToken);
            _logger.LogInformation($"Override saved for page {model.PageId}");
            return result.ToActionResult(this);
        }
    }
}
