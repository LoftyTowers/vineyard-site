using System;
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
