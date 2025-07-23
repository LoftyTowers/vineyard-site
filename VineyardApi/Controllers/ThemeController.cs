using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("theme")]
    public class ThemeController : ControllerBase
    {
        private readonly VineyardDbContext _context;

        public ThemeController(VineyardDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTheme()
        {
            var defaults = await _context.ThemeDefaults.ToListAsync();
            var overrides = await _context.ThemeOverrides.ToListAsync();
            var result = defaults.ToDictionary(d => d.Key, d => d.Value);
            foreach (var ovr in overrides.OrderByDescending(o => o.UpdatedAt))
            {
                var key = defaults.FirstOrDefault(d => d.Id == ovr.ThemeDefaultId)?.Key;
                if (key != null) result[key] = ovr.Value;
            }
            return Ok(result);
        }

        [Authorize]
        [HttpPost("override")]
        public async Task<IActionResult> SaveOverride([FromBody] ThemeOverride model)
        {
            model.UpdatedAt = DateTime.UtcNow;
            var existing = await _context.ThemeOverrides.FirstOrDefaultAsync(t => t.ThemeDefaultId == model.ThemeDefaultId);
            if (existing == null)
                _context.ThemeOverrides.Add(model);
            else
            {
                existing.Value = model.Value;
                existing.UpdatedAt = model.UpdatedAt;
                existing.UpdatedById = model.UpdatedById;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
