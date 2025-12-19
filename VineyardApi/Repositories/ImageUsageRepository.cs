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
            var usages = await _context.ImageUsages
                .Where(iu => iu.EntityType == "Page" && iu.EntityKey == route && iu.Source == source)
                .ToListAsync(cancellationToken);
            _context.ImageUsages.RemoveRange(usages);
        }

        public void AddRange(IEnumerable<ImageUsage> usages)
        {
            _context.ImageUsages.AddRange(usages);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
