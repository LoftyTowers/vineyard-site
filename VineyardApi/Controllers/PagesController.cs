using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("pages")]
    public class PagesController : ControllerBase
    {
        private readonly VineyardDbContext _context;

        public PagesController(VineyardDbContext context)
        {
            _context = context;
        }

        [HttpGet("{route}")]
        public async Task<IActionResult> GetPage(string route)
        {
            var page = await _context.Pages.Include(p => p.Overrides)
                .FirstOrDefaultAsync(p => p.Route == route);
            if (page == null) return NotFound();

            var overrideContent = page.Overrides.OrderByDescending(o => o.UpdatedAt).FirstOrDefault();
            var merged = (page.DefaultContent?.DeepClone() as JsonObject) ?? new JsonObject();
            if (overrideContent?.OverrideContent != null)
            {
                merged = MergeJson(merged, overrideContent.OverrideContent);
            }
            return Ok(merged);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] PageOverride model)
        {
            model.UpdatedAt = DateTime.UtcNow;
            var existing = await _context.PageOverrides.FirstOrDefaultAsync(p => p.PageId == model.PageId);
            if (existing == null)
                _context.PageOverrides.Add(model);
            else
            {
                existing.OverrideContent = model.OverrideContent;
                existing.UpdatedAt = model.UpdatedAt;
                existing.UpdatedById = model.UpdatedById;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        private static JsonObject MergeJson(JsonObject original, JsonObject update)
        {
            foreach (var prop in update)
            {
                original[prop.Key] = prop.Value;
            }
            return original;
        }
    }
}
