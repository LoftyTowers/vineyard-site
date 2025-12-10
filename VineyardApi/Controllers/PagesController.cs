using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Domain.Content;
using VineyardApi.Infrastructure;
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
        public async Task<IActionResult> GetPage(string route)
        {
            var result = await _service.GetPageContentAsync(route);
            if (result == null)
            {
                return Result<PageContent>.Failure(ErrorCode.NotFound, "Page not found").ToActionResult(this);
            }

            return Result<PageContent>.Success(result).ToActionResult(this);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] PageOverride model)
        {
            await _service.SaveOverrideAsync(model);
            return Result.Success().ToActionResult(this);
        }
    }
}
