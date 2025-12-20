using VineyardApi.Domain.Content;
using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IPageService
    {
        Task<Result<PageContent>> GetPageContentAsync(string route, CancellationToken cancellationToken = default);
        Task<Result<PageContent>> GetDraftContentAsync(string route, CancellationToken cancellationToken = default);
        Task<Result<PageContent>> UpdateHeroImageAsync(string route, Guid imageId, CancellationToken cancellationToken = default);
        Task<Result> SaveOverrideAsync(PageOverride model, CancellationToken cancellationToken = default);
        Task<Result> AutosaveDraftAsync(string route, PageContent content, CancellationToken cancellationToken = default);
        Task<Result<PageContent>> PublishDraftAsync(string route, CancellationToken cancellationToken = default);
        Task<Result> DiscardDraftAsync(string route, CancellationToken cancellationToken = default);
        Task<Result<List<PageVersionSummary>>> GetPublishedVersionsAsync(string route, CancellationToken cancellationToken = default);
        Task<Result<PageVersionContentResponse>> GetPublishedVersionContentAsync(string route, Guid versionId, CancellationToken cancellationToken = default);
        Task<Result<PageContent>> RollbackToVersionAsync(string route, Guid versionId, CancellationToken cancellationToken = default);
    }
}
