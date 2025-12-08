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

        public async Task<Image> SaveImageAsync(Image img)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"ImageName", img.Name}});
            img.Id = Guid.NewGuid();
            img.CreatedAt = DateTime.UtcNow;
            _logger.LogInformation("Persisting image {ImageName}", img.Name);
            _repository.AddImage(img);
            await _repository.SaveChangesAsync();
            return img;
        }
    }
}
