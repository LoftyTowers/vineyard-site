using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IImageRepository
    {
        void AddImage(Image image);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
