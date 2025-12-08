using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;
using System.Collections.Generic;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _service;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(IImageService service, ILogger<ImagesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveImage([FromBody] Image img)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"ImageName", img.Name}});
            _logger.LogInformation("Saving image {ImageName}", img.Name);
            var saved = await _service.SaveImageAsync(img);
            return Ok(saved);
        }
    }
}
