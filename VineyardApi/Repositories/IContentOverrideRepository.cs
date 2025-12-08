using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IContentOverrideRepository
    {
        Task<List<ContentOverride>> GetLatestPublishedAsync(string route, CancellationToken cancellationToken);
        Task<ContentOverride?> GetDraftAsync(Guid pageId, string blockKey, CancellationToken cancellationToken);
        Task<ContentOverride?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<ContentOverride>> GetHistoryAsync(string route, string blockKey, CancellationToken cancellationToken);
        void Add(ContentOverride model);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
