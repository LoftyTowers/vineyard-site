using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IContentOverrideService
    {
        Task<Result<Dictionary<string, string>>> GetPublishedOverridesAsync(string route, CancellationToken cancellationToken);
        Task<Result> SaveDraftAsync(ContentOverride model, CancellationToken cancellationToken);
        Task<Result> PublishAsync(ContentOverride model, CancellationToken cancellationToken);
        Task<Result> PublishDraftAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<List<ContentOverride>>> GetHistoryAsync(string route, string blockKey, CancellationToken cancellationToken);
        Task<Result> RevertAsync(Guid id, Guid changedById, CancellationToken cancellationToken);
    }
}
