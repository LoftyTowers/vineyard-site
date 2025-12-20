using System;
using Microsoft.EntityFrameworkCore;
using VineyardApi.Data;
using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly VineyardDbContext _context;
        public ImageRepository(VineyardDbContext context)
        {
            _context = context;
        }

        public void AddImage(Image image)
        {
            _context.Images.Add(image);
        }

        public async Task<List<Image>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Images
                .Where(i => i.IsActive)
                .OrderBy(i => i.CreatedUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<Guid, Image>> GetActiveByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _context.Images
                .Where(i => ids.Contains(i.Id) && i.IsActive)
                .ToDictionaryAsync(i => i.Id, cancellationToken);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
