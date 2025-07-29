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

        public async Task<List<ContentOverride>> GetLatestPublishedAsync(string route)
        {
            return await _context.ContentOverrides
                .Include(o => o.Page)
                .Where(o => o.Page!.Route == route && o.Status == "published")
                .GroupBy(o => o.BlockKey)
                .Select(g => g.OrderByDescending(o => o.Timestamp).First())
                .ToListAsync();
        }

        public Task<ContentOverride?> GetDraftAsync(Guid pageId, string blockKey)
        {
            return _context.ContentOverrides
                .FirstOrDefaultAsync(o => o.PageId == pageId && o.BlockKey == blockKey && o.Status == "draft");
        }

        public Task<ContentOverride?> GetByIdAsync(Guid id)
        {
            return _context.ContentOverrides.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<ContentOverride>> GetHistoryAsync(string route, string blockKey)
        {
            return await _context.ContentOverrides
                .Include(o => o.Page)
                .Where(o => o.Page!.Route == route && o.BlockKey == blockKey)
                .OrderByDescending(o => o.Timestamp)
                .ToListAsync();
        }

        public void Add(ContentOverride model)
        {
            _context.ContentOverrides.Add(model);
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
