using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/pages")]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _service;

        public PagesController(IPageService service)
        {
            _service = service;
        }

        [HttpGet("{route}")]
        public async Task<IActionResult> GetPage(string route, CancellationToken cancellationToken)
        {
            var result = await _service.GetPageContentAsync(route, cancellationToken);
            if (result.IsFailure)
            {
                return result.Error == ErrorCode.NotFound ? NotFound() : StatusCode(500);
            }

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] PageOverride model, CancellationToken cancellationToken)
        {
            var result = await _service.SaveOverrideAsync(model, cancellationToken);
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok();
        }
    }
}
