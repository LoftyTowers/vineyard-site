using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;
using System.Collections.Generic;
using FluentValidation;

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
        public async Task<IActionResult> SaveImage([FromBody] Image img, CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"ImageUrl", img.Url}});
            _logger.LogInformation("Saving image {ImageUrl}", img.Url);
            var validationResult = await _validator.ValidateAsync(img, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var saved = await _service.SaveImageAsync(img);
            return Ok(saved);
        }
    }
}
