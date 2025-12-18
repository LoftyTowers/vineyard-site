using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IImageRepository
    {
        void AddImage(Image image);
        Task<List<Image>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, Image>> GetActiveByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
