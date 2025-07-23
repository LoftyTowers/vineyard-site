using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IImageService
    {
        Task<Image> SaveImageAsync(Image img);
    }
}
