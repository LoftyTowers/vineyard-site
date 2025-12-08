using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IImageService
    {
        Task<Result<Image>> SaveImageAsync(Image img, CancellationToken cancellationToken);
    }
}
