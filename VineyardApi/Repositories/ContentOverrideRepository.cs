using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task<List<ContentOverride>> GetLatestPublishedAsync(string route, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.ContentOverrides
                    .Include(o => o.Page)
                    .Where(o => o.Page!.Route == route && o.Status == "published")
                    .GroupBy(o => o.BlockKey)
                    .Select(g => g.OrderByDescending(o => o.Timestamp).First())
                    .ToListAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<ContentOverride?> GetDraftAsync(Guid pageId, string blockKey, CancellationToken cancellationToken = default)
        {
            try
            {
                return _context.ContentOverrides
                    .FirstOrDefaultAsync(o => o.PageId == pageId && o.BlockKey == blockKey && o.Status == "draft", cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<ContentOverride?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return _context.ContentOverrides.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ContentOverride>> GetHistoryAsync(string route, string blockKey, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.ContentOverrides
                    .Include(o => o.Page)
                    .Include(o => o.ChangedBy)
                    .Where(o => o.Page!.Route == route && o.BlockKey == blockKey)
                    .OrderByDescending(o => o.Timestamp)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Add(ContentOverride model)
        {
            try
            {
                _context.ContentOverrides.Add(model);
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
