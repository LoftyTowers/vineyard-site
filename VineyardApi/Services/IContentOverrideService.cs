using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IContentOverrideService
    {
        Task<Dictionary<string, string>> GetPublishedOverridesAsync(string route);
        Task SaveDraftAsync(ContentOverride model);
        Task PublishAsync(ContentOverride model);
        Task PublishDraftAsync(Guid id);
        Task<List<ContentOverride>> GetHistoryAsync(string route, string blockKey);
        Task RevertAsync(Guid id, Guid changedById);
    }
}
