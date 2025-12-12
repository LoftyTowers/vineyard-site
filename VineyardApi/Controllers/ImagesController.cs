using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _service;
        private readonly ILogger<ImagesController> _logger;

        private readonly IValidator<Image> _validator;

        public ImagesController(IImageService service, ILogger<ImagesController> logger, IValidator<Image> validator)
        {
            _service = service;
            _logger = logger;
            _validator = validator;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveImageAsync([FromBody] Image img, CancellationToken cancellationToken)
        {
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["ImageId"] = img.Id == Guid.Empty ? null : img.Id,
                ["ImageUrl"] = img.Url ?? string.Empty
            });

            try
            {
                var validation = await _validator.ValidateAsync(img, cancellationToken);
                if (!validation.IsValid)
                {
                    return ResultMapper.FromValidationResult(this, validation);
                }

                var saved = await _service.SaveImageAsync(img, cancellationToken);
                return ResultMapper.ToActionResult(this, saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save image {ImageUrl}", img.Url);
                return ResultMapper.ToActionResult(this, Result<Image>.Failure(ErrorCode.Unknown, "Failed to save image"));
            }
        }
    }
}
