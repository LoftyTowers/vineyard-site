using System.Text.Json.Nodes;
using VineyardApi.Models;
using VineyardApi.Repositories;

namespace VineyardApi.Services
{
    public class PageService : IPageService
    {
        private readonly IPageRepository _repository;

        public PageService(IPageRepository repository)
        {
            _repository = repository;
        }

        public async Task<JsonObject?> GetPageContentAsync(string route)
        {
            var page = await _repository.GetPageWithOverridesAsync(route);
            if (page == null) return null;

            var overrideContent = page.Overrides
                .OrderByDescending(o => o.UpdatedAt)
                .FirstOrDefault();
            var merged = (page.DefaultContent?.DeepClone() as JsonObject) ?? new JsonObject();
            if (overrideContent?.OverrideContent != null)
            {
                merged = MergeJson(merged, overrideContent.OverrideContent);
            }
            return merged;
        }

        public async Task SaveOverrideAsync(PageOverride model)
        {
            model.UpdatedAt = DateTime.UtcNow;
            var existing = await _repository.GetPageOverrideByPageIdAsync(model.PageId);
            if (existing == null)
            {
                _repository.AddPageOverride(model);
            }
            else
            {
                existing.OverrideContent = model.OverrideContent;
                existing.UpdatedAt = model.UpdatedAt;
                existing.UpdatedById = model.UpdatedById;
            }
            await _repository.SaveChangesAsync();
        }

        private static JsonObject MergeJson(JsonObject original, JsonObject update)
        {
            foreach (var prop in update)
            {
                original[prop.Key] = prop.Value;
            }
            return original;
        }
    }
}
