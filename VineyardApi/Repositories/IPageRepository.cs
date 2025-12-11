using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IPageRepository
    {
        Task<Page?> GetPageWithOverridesAsync(string route, CancellationToken cancellationToken = default);
        Task<PageOverride?> GetPageOverrideByPageIdAsync(Guid pageId, CancellationToken cancellationToken = default);
        void AddPageOverride(PageOverride model);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
