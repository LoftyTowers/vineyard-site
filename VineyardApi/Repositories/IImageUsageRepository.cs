using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IImageUsageRepository
    {
        Task DeletePageUsagesAsync(string route, string source, CancellationToken cancellationToken = default);
        void AddRange(IEnumerable<ImageUsage> usages);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
