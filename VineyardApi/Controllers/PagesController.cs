using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using VineyardApi.Services;
using FluentValidation;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("api/pages")]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _service;
        private readonly IValidator<PageOverride> _validator;

        public PagesController(IPageService service, IValidator<PageOverride> validator)
        {
            _service = service;
            _validator = validator;
        }

        [HttpGet("{route}")]
        public async Task<IActionResult> GetPage(string route)
        {
            var result = await _service.GetPageContentAsync(route);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] PageOverride model, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(model, cancellationToken);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            await _service.SaveOverrideAsync(model);
            return Ok();
        }
    }
}
