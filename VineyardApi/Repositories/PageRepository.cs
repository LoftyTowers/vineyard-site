using System;
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

        public async Task<Page?> GetPageWithOverridesAsync(string route, CancellationToken cancellationToken = default)
        {
            var page = await _context.Pages
                .Include(p => p.CurrentVersion)
                .Include(p => p.Overrides)
                .FirstOrDefaultAsync(p => p.Route == route, cancellationToken);
            return page;
        }

        public async Task<Page?> GetPageWithVersionsAsync(string route, CancellationToken cancellationToken = default)
        {
            return await _context.Pages
                .Include(p => p.Versions)
                .FirstOrDefaultAsync(p => p.Route == route, cancellationToken);
        }

        public async Task<PageOverride?> GetPageOverrideByPageIdAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            return await _context.PageOverrides
                .FirstOrDefaultAsync(p => p.PageId == pageId, cancellationToken);
        }

        public async Task<Page?> GetPageByIdAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            return await _context.Pages
                .FirstOrDefaultAsync(p => p.Id == pageId, cancellationToken);
        }

        public async Task<List<PageVersion>> GetPublishedVersionsAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            return await _context.PageVersions
                .Where(v => v.PageId == pageId && (v.Status == PageVersionStatus.Published || v.Status == PageVersionStatus.Archived))
                .OrderByDescending(v => v.VersionNo)
                .ToListAsync(cancellationToken);
        }

        public Task<PageVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken = default)
        {
            return _context.PageVersions
                .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);
        }

        public void AddPageOverride(PageOverride model)
        {
            _context.PageOverrides.Add(model);
        }

        public void AddPageVersion(PageVersion version)
        {
            _context.PageVersions.Add(version);
        }

        public void RemovePageVersion(PageVersion version)
        {
            _context.PageVersions.Remove(version);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
