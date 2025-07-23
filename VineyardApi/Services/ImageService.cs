using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _repository;

        public ImageService(IImageRepository repository)
        {
            _repository = repository;
        }

        public async Task<Image> SaveImageAsync(Image img)
        {
            img.Id = Guid.NewGuid();
            img.CreatedAt = DateTime.UtcNow;
            _repository.AddImage(img);
            await _repository.SaveChangesAsync();
            return img;
        }
    }
}
