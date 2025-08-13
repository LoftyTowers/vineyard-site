using VineyardApi.Domain.Content;
using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IPageService
    {
        Task<PageContent?> GetPageContentAsync(string route);
        Task SaveOverrideAsync(PageOverride model);
    }
}
