using Microsoft.Extensions.Logging;
using VineyardApi.Infrastructure;
using VineyardApi.Models;
using VineyardApi.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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
                using var scope = _logger.BeginScope(new Dictionary<string, object>{{"ImageUrl", img.Url}});
                img.Id = Guid.NewGuid();
                img.CreatedAt = DateTime.UtcNow;
                _logger.LogInformation("Persisting image {ImageUrl}", img.Url);
            _repository.AddImage(img);
                await _repository.SaveChangesAsync(cancellationToken);
                return Result<Image>.Success(img);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image {ImageUrl}", img.Url);
                return Result<Image>.Failure(ErrorCode.Unexpected);
            }
        }
    }
}
