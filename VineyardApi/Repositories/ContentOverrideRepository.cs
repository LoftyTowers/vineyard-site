using Microsoft.EntityFrameworkCore;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public class ContentOverrideRepository : IContentOverrideRepository
    {
        private readonly VineyardDbContext _context;
        public ContentOverrideRepository(VineyardDbContext context)
        {
            _context = context;
        }

        public async Task<List<ContentOverride>> GetLatestPublishedAsync(string route, CancellationToken cancellationToken)
        {
            return await _context.ContentOverrides
                .Include(o => o.Page)
                .Where(o => o.Page!.Route == route && o.Status == "published")
                .GroupBy(o => o.BlockKey)
                .Select(g => g.OrderByDescending(o => o.Timestamp).First())
                .ToListAsync(cancellationToken);
        }

        public Task<ContentOverride?> GetDraftAsync(Guid pageId, string blockKey, CancellationToken cancellationToken)
        {
            return _context.ContentOverrides
                .FirstOrDefaultAsync(o => o.PageId == pageId && o.BlockKey == blockKey && o.Status == "draft", cancellationToken);
        }

        public Task<ContentOverride?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return _context.ContentOverrides.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<List<ContentOverride>> GetHistoryAsync(string route, string blockKey, CancellationToken cancellationToken)
        {
            return await _context.ContentOverrides
                .Include(o => o.Page)
                .Include(o => o.ChangedBy)
                .Where(o => o.Page!.Route == route && o.BlockKey == blockKey)
                .OrderByDescending(o => o.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public void Add(ContentOverride model)
        {
            _context.ContentOverrides.Add(model);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
    }
}
