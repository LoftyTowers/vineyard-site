using VineyardApi.Models;

using VineyardApi.Infrastructure;

namespace VineyardApi.Services
{
    public interface IContentOverrideService
    {
        Task<Result<Dictionary<string, string>>> GetPublishedOverridesAsync(string route, CancellationToken cancellationToken = default);
        Task<Result> SaveDraftAsync(ContentOverride model, CancellationToken cancellationToken = default);
        Task<Result> PublishAsync(ContentOverride model, CancellationToken cancellationToken = default);
        Task<Result> PublishDraftAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<List<ContentOverride>>> GetHistoryAsync(string route, string blockKey, CancellationToken cancellationToken = default);
        Task<Result> RevertAsync(Guid id, Guid changedById, CancellationToken cancellationToken = default);
    }
}
