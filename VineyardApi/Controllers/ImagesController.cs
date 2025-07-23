using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Controllers
{
    [ApiController]
    [Route("images")]
    public class ImagesController : ControllerBase
    {
        private readonly VineyardDbContext _context;
        public ImagesController(VineyardDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveImage([FromBody] Image img)
        {
            img.Id = Guid.NewGuid();
            img.CreatedAt = DateTime.UtcNow;
            _context.Images.Add(img);
            await _context.SaveChangesAsync();
            return Ok(img);
        }
    }
}
