using System.Text.Json.Nodes;
using VineyardApi.Models;

namespace VineyardApi.Services
{
    public interface IPageService
    {
        Task<JsonObject?> GetPageContentAsync(string route);
        Task SaveOverrideAsync(PageOverride model);
    }
}
