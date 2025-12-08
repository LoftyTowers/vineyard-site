using VineyardApi.Domain.Content;
using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IPageService
    {
        Task<Result<PageContent>> GetPageContentAsync(string route, CancellationToken cancellationToken = default);
        Task<Result> SaveOverrideAsync(PageOverride model, CancellationToken cancellationToken = default);
    }
}
