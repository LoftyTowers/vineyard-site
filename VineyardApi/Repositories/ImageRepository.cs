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

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
    }
}
