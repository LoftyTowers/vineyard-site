using Microsoft.EntityFrameworkCore;
using System.Linq;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public class ImageUsageRepository : IImageUsageRepository
    {
        private readonly VineyardDbContext _context;

        public ImageUsageRepository(VineyardDbContext context)
        {
            _context = context;
        }

        public async Task DeletePageUsagesAsync(string route, string source, CancellationToken cancellationToken = default)
        {
            try
            {
                var usages = await _context.ImageUsages
                    .Where(iu => iu.EntityType == "Page" && iu.EntityKey == route && iu.Source == source)
                    .ToListAsync(cancellationToken);
                _context.ImageUsages.RemoveRange(usages);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddRange(IEnumerable<ImageUsage> usages)
        {
            try
            {
                _context.ImageUsages.AddRange(usages);
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
