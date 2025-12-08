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

        public async Task<Page?> GetPageWithOverridesAsync(string route, CancellationToken cancellationToken)
        {
            return await _context.Pages
                .Include(p => p.Overrides)
                .FirstOrDefaultAsync(p => p.Route == route, cancellationToken);
        }

        public async Task<PageOverride?> GetPageOverrideByPageIdAsync(Guid pageId, CancellationToken cancellationToken)
        {
            return await _context.PageOverrides
                .FirstOrDefaultAsync(p => p.PageId == pageId, cancellationToken);
        }

        public void AddPageOverride(PageOverride model)
        {
            _context.PageOverrides.Add(model);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
    }
}
