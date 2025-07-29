using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IContentOverrideRepository
    {
        Task<List<ContentOverride>> GetLatestPublishedAsync(string route);
        Task<ContentOverride?> GetDraftAsync(Guid pageId, string blockKey);
        Task<ContentOverride?> GetByIdAsync(Guid id);
        Task<List<ContentOverride>> GetHistoryAsync(string route, string blockKey);
        void Add(ContentOverride model);
        Task<int> SaveChangesAsync();
    }
}
