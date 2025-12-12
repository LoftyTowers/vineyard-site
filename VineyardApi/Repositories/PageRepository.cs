using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task<Page?> GetPageWithOverridesAsync(string route, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Pages
                    .Include(p => p.Overrides)
                    .FirstOrDefaultAsync(p => p.Route == route, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PageOverride?> GetPageOverrideByPageIdAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.PageOverrides
                    .FirstOrDefaultAsync(p => p.PageId == pageId, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddPageOverride(PageOverride model)
        {
            try
            {
                _context.PageOverrides.Add(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
