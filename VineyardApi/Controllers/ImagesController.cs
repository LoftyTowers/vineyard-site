using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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
                ["ImageUrl"] = img.PublicUrl ?? string.Empty
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
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Image save cancelled for {ImageUrl}", img.PublicUrl);
                return ResultMapper.ToActionResult(this, Result<Image>.Failure(ErrorCode.Cancelled, "Request cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save image {ImageUrl}", img.PublicUrl);
                return ResultMapper.ToActionResult(this, Result<Image>.Failure(ErrorCode.Unexpected, "Failed to save image"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetImagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
                using var scope = _logger.BeginScope(new Dictionary<string, object?>
                {
                    ["CorrelationId"] = correlationId
                });
                var result = await _service.GetActiveImagesAsync(cancellationToken);
                if (result.IsFailure)
                {
                    return ResultMapper.ToActionResult(this, Result<List<ImageListItem>>.Failure(result.Error, result.Message));
                }

                var items = result.Value!
                    .Select(img =>
                    {
                        var mappedUrl = ResolveImageUrl(img);
                        return new ImageListItem
                        {
                            Id = img.Id,
                            Url = mappedUrl,
                            ThumbnailUrl = mappedUrl,
                            Alt = img.AltText,
                            Caption = img.Caption,
                            Width = img.Width,
                            Height = img.Height
                        };
                    })
                    .ToList();

                return ResultMapper.ToActionResult(this, Result<List<ImageListItem>>.Ok(items));
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Image list request cancelled");
                return ResultMapper.ToActionResult(this, Result<List<ImageListItem>>.Failure(ErrorCode.Cancelled, "Request cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load images");
                return ResultMapper.ToActionResult(this, Result<List<ImageListItem>>.Failure(ErrorCode.Unexpected, "Failed to load images"));
            }
        }

        private static string ResolveImageUrl(Image img)
        {
            if (!string.IsNullOrWhiteSpace(img.PublicUrl))
            {
                if (img.PublicUrl.StartsWith("assets/", StringComparison.OrdinalIgnoreCase))
                {
                    return "/" + img.PublicUrl;
                }

                return img.PublicUrl;
            }

            if (!string.IsNullOrWhiteSpace(img.StorageKey))
            {
                return $"/images/{img.StorageKey}";
            }

            return string.Empty;
        }
    }
}
