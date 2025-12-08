using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _service;
        private readonly IValidator<Image> _validator;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(IImageService service, IValidator<Image> validator, ILogger<ImagesController> logger)
        {
            _service = service;
            _validator = validator;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveImage([FromBody] Image img, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = HttpContext.TraceIdentifier,
                ["ImageId"] = img.Id == Guid.Empty ? null : img.Id,
                ["ImageUrl"] = img.Url
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
