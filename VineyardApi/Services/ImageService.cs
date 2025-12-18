using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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
                using var scope = _logger.BeginScope(new Dictionary<string, object>{{"ImageUrl", img.PublicUrl}});
                if (img.Id == Guid.Empty)
                {
                    img.Id = Guid.NewGuid();
                    img.CreatedUtc = DateTime.UtcNow;
                }
                _logger.LogInformation("Persisting image {ImageUrl}", img.PublicUrl);
                _repository.AddImage(img);
                await _repository.SaveChangesAsync(cancellationToken);
                return Result<Image>.Ok(img);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image {ImageUrl}", img.PublicUrl);
                return Result<Image>.Failure(ErrorCode.Unexpected);
            }
        }

        public async Task<Result<List<Image>>> GetActiveImagesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var images = await _repository.GetActiveAsync(cancellationToken);
                return Result<List<Image>>.Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load images");
                return Result<List<Image>>.Failure(ErrorCode.Unexpected, "ImageReadFailed");
            }
        }
    }
}
