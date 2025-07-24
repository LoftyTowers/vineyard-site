using VineyardApi.Models;

namespace VineyardApi.Repositories
{
    public interface IPageRepository
    {
        Task<Page?> GetPageWithOverridesAsync(string route);
        Task<PageOverride?> GetPageOverrideByPageIdAsync(Guid pageId);
        void AddPageOverride(PageOverride model);
        Task<int> SaveChangesAsync();
    }
}
