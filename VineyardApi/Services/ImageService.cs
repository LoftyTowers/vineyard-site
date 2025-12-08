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

        public async Task<Result<Image>> SaveImageAsync(Image img, CancellationToken cancellationToken)
        {
            img.Id = Guid.NewGuid();
            img.CreatedAt = DateTime.UtcNow;
            _repository.AddImage(img);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<Image>.Ok(img);
        }
    }
}
