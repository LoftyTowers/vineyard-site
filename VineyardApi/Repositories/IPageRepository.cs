using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IPageRepository
    {
        Task<Page?> GetPageWithOverridesAsync(string route, CancellationToken cancellationToken = default);
        Task<Page?> GetPageWithVersionsAsync(string route, CancellationToken cancellationToken = default);
        Task<Page?> GetPageByIdAsync(Guid pageId, CancellationToken cancellationToken = default);
        Task<PageOverride?> GetPageOverrideByPageIdAsync(Guid pageId, CancellationToken cancellationToken = default);
        Task<List<PageVersion>> GetPublishedVersionsAsync(Guid pageId, CancellationToken cancellationToken = default);
        Task<PageVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken = default);
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        void AddPageOverride(PageOverride model);
        void AddPageVersion(PageVersion version);
        void RemovePageVersion(PageVersion version);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
