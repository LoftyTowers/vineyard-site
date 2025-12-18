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
            try
            {
                _context.Images.Add(image);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Image>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Images
                    .Where(i => i.IsActive)
                    .OrderBy(i => i.CreatedUtc)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Dictionary<Guid, Image>> GetActiveByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Images
                    .Where(i => ids.Contains(i.Id) && i.IsActive)
                    .ToDictionaryAsync(i => i.Id, cancellationToken);
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
