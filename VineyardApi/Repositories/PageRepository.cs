using Microsoft.EntityFrameworkCore;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public class PageRepository : IPageRepository
    {
        private readonly VineyardDbContext _context;

        public PageRepository(VineyardDbContext context)
        {
            _context = context;
        }

        public async Task<Page?> GetPageWithOverridesAsync(string route)
        {
            return await _context.Pages
                .Include(p => p.Overrides)
                .FirstOrDefaultAsync(p => p.Route == route);
        }

        public async Task<PageOverride?> GetPageOverrideByPageIdAsync(Guid pageId)
        {
            return await _context.PageOverrides
                .FirstOrDefaultAsync(p => p.PageId == pageId);
        }

        public void AddPageOverride(PageOverride model)
        {
            _context.PageOverrides.Add(model);
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
