using VineyardApi.Domain.Content;
using VineyardApi.Models;
using VineyardApi.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace VineyardApi.Services
{
    public class PageService : IPageService
    {
        private readonly IPageRepository _repository;
        private readonly ILogger<PageService> _logger;

        public PageService(IPageRepository repository, ILogger<PageService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PageContent?> GetPageContentAsync(string route)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"PageRoute", route}});
            var page = await _repository.GetPageWithOverridesAsync(route);
            if (page == null) return null;

            var overrideContent = page.Overrides
                .OrderByDescending(o => o.UpdatedAt)
                .FirstOrDefault();

            return overrideContent?.OverrideContent ?? page.DefaultContent;
        }

        public async Task SaveOverrideAsync(PageOverride model)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"PageId", model.PageId}});
            model.UpdatedAt = DateTime.UtcNow;
            var existing = await _repository.GetPageOverrideByPageIdAsync(model.PageId);
            if (existing == null)
            {
                _logger.LogInformation("Creating override for page {PageId}", model.PageId);
                _repository.AddPageOverride(model);
            }
            else
            {
                _logger.LogInformation("Updating override for page {PageId}", model.PageId);
                existing.OverrideContent = model.OverrideContent;
                existing.UpdatedAt = model.UpdatedAt;
                existing.UpdatedById = model.UpdatedById;
            }
            await _repository.SaveChangesAsync();
        }

        
    }
}
