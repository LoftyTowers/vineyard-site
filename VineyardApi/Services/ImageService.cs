using Microsoft.Extensions.Logging;
using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _repository;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IImageRepository repository, ILogger<ImageService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<Image>> SaveImageAsync(Image img, CancellationToken cancellationToken = default)
        {
            try
            {
                img.Id = Guid.NewGuid();
                img.CreatedAt = DateTime.UtcNow;
                _repository.AddImage(img);
                await _repository.SaveChangesAsync(cancellationToken);
                return Result<Image>.Success(img);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image {ImageName}", img.Name);
                return Result<Image>.Failure(ErrorCode.Unexpected);
            }
        }
    }
}
