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
        public ImagesController(IImageService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveImage([FromBody] Image img, CancellationToken cancellationToken)
        {
            var saved = await _service.SaveImageAsync(img, cancellationToken);
            if (saved.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok(saved.Value);
        }
    }
}
