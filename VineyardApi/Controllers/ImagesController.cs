using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;
using FluentValidation;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _service;
        private readonly IValidator<Image> _validator;

        public ImagesController(IImageService service, IValidator<Image> validator)
        {
            _service = service;
            _validator = validator;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveImage([FromBody] Image img, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(img, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var saved = await _service.SaveImageAsync(img);
            return Ok(saved);
        }
    }
}
