using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IContentOverrideRepository
    {
        Task<List<ContentOverride>> GetLatestPublishedAsync(string route, CancellationToken cancellationToken = default);
        Task<ContentOverride?> GetDraftAsync(Guid pageId, string blockKey, CancellationToken cancellationToken = default);
        Task<ContentOverride?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ContentOverride>> GetHistoryAsync(string route, string blockKey, CancellationToken cancellationToken = default);
        void Add(ContentOverride model);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
